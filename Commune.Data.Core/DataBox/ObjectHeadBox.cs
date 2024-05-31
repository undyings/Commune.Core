using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Commune.Data
{
	public class ObjectHeadBox
	{
		public readonly SingleIndexLink<ObjectRow> ObjectById;
		public readonly MultiIndexLink<ObjectRow> ObjectsByJsonId;

		public readonly UniqueChecker ObjectUniqueChecker;

		protected DataLayer dataLayer;
		readonly Func<BoxDbContext, IQueryable<ObjectRow>> objectRowsLoader;

		public ObjectHeadBox(DataLayer dataLayer, BoxDbContext context, Func<BoxDbContext, IQueryable<ObjectRow>> objectRowsLoader)
		{
			this.dataLayer = dataLayer;
			this.objectRowsLoader = objectRowsLoader;

			ObjectRow[] objectRows = objectRowsLoader(context).ToArray();

			TableLink<ObjectRow> objectsLink = new(objectRows, ObjectType.ObjectById, ObjectType.ObjectsByJsonIds);

			this.ObjectById = new SingleIndexLink<ObjectRow>(objectsLink, ObjectType.ObjectById);
			this.ObjectsByJsonId = new MultiIndexLink<ObjectRow>(objectsLink, ObjectType.ObjectsByJsonIds);

			this.ObjectUniqueChecker = new UniqueChecker(this);
		}

		public ObjectHeadBox(DataLayer dataLayer, BoxDbContext context) :
			this(dataLayer, context, db => Array.Empty<ObjectRow>().AsQueryable())
		{
		}


		public ObjectRow CreateObjectRow(int typeId, string jsonIds, DateTime? actFrom)
		{
			ObjectRow obj = new()
			{
				ObjectId = dataLayer.GeneratePrimaryKey(BoxTableNames.ObjectTable),
				TypeId = typeId,
				JsonId = jsonIds,
				ActFrom = actFrom
			};

			return obj;
		}

		public int CreateAndAddObject(int typeKind, string jsonIds, DateTime? actFrom)
		{
			ObjectRow newRow = CreateObjectRow(typeKind, jsonIds, actFrom);
			ObjectById.TableLink.AddRow(newRow);

			return newRow.ObjectId;
		}

		public int? CreateUniqueObject(BoxDbContext context, int typeKind, string jsonId, DateTime? actFrom)
		{
			if (!ObjectUniqueChecker.IsUniqueKey(context, null, typeKind, jsonId, actFrom))
				return null;

			return CreateAndAddObject(typeKind, jsonId, actFrom);
		}

		//Extension
		public int[] AllObjectIds
		{
			get { return ObjectById.TableLink.AllRows.Select(obj => obj.ObjectId).ToArray(); }
		}

		public bool IsKindObject(long objectId, params int[] requiredKinds)
		{
			ObjectRow? row =  ObjectById.Row(objectId);
			if (row == null)
				return false;

			return requiredKinds.Contains(row.TypeId);
		}

		public virtual void Update(BoxDbContext context, bool saveChangesToDb)
		{
			ObjectRow[] dbObjectRows = objectRowsLoader(context).ToArray();
			ObjectById.TableLink.SyncChanges(context.Objects, dbObjectRows, ObjectType.SyncChanges);

			if (saveChangesToDb)
				context.SaveChanges();
		}

		public virtual long DataChangeTick
		{
			get
			{
				return ObjectById.TableLink.DataChangeTick;
			}
		}

		public long RowListChangeTick
		{
			get { return ObjectById.TableLink.RowListChangeTick; }
		}

	}
}
