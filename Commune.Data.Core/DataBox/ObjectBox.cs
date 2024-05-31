using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class ObjectBox : ObjectHeadBox
	{
		static PropertyRow[] LoadPropertyRows(BoxDbContext context, int[] objectIds)
		{
			return context.Properties.Where(p => objectIds.Contains(p.ObjectId))
				.OrderBy(p => p.ObjectId).ThenBy(p => p.TypeId).ThenBy(p => p.PropertyIndex)
				.ToArray();
		}

		public readonly OrderedIndexLink<PropertyRow> PropertiesByObjectIdWithKind;
		public readonly MultiIndexLink<PropertyRow> PropertiesByObjectId;

		public ObjectBox(DataLayer dataLayer, BoxDbContext context, Func<BoxDbContext, IQueryable<ObjectRow>> rowsLoader) :
			base(dataLayer, context, rowsLoader)
		{
			PropertyRow[] propertyRows = ObjectBox.LoadPropertyRows(context, base.AllObjectIds);

			TableLink<PropertyRow> propertyTable = new(propertyRows, PropertyType.PropertyById, PropertyType.PropertiesByObjectIdAndTypeId, PropertyType.PropertiesByObjectId);

			this.PropertiesByObjectIdWithKind = new OrderedIndexLink<PropertyRow>(propertyTable, PropertyType.PropertiesByObjectIdAndTypeId);
			this.PropertiesByObjectId = new MultiIndexLink<PropertyRow>(propertyTable, PropertyType.PropertiesByObjectId);
		}

		public ObjectBox(DataLayer dataLayer, BoxDbContext context) :
			this(dataLayer, context, db => Array.Empty<ObjectRow>().AsQueryable())
		{
		}

		public TableLink<PropertyRow> PropertyTable
		{
			get { return PropertiesByObjectId.TableLink; }
		}

		public PropertyRow CreatePropertyRow(int objectId, int typeId, int propertyIndex)
		{
			PropertyRow property = new()
			{
				PropertyId = dataLayer.GeneratePrimaryKey(BoxTableNames.PropertyTable),
				ObjectId = objectId,
				TypeId = typeId,
				PropertyIndex = propertyIndex
			};

			return property;
		}


		public override void Update(BoxDbContext context, bool syncChangesWithDb)
		{
			base.Update(context, false);

			PropertyRow[] dbPropertyRows = ObjectBox.LoadPropertyRows(context, base.AllObjectIds);
			PropertiesByObjectId.TableLink.SyncChanges(context.Properties, dbPropertyRows, PropertyType.SyncChanges);

			if (syncChangesWithDb)
				context.SaveChanges();
		}

		public override long DataChangeTick
		{
			get
			{
				return base.DataChangeTick +  PropertyTable.DataChangeTick + PropertyTable.RowListChangeTick;
			}
		}
	}
}
