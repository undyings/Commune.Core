using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Commune.Data
{
	public class DataBox
	{
		public static Tuple<string, int> LoadObjectMaxId(BoxDbContext dbContext)
		{
			int maxId = dbContext.Objects.Max(obj => (int?)obj.ObjectId) ?? 0;
			//int maxId = dbContext.Objects.OrderByDescending(obj => obj.ObjectId).Select(obj => obj.ObjectId).Take(1).FirstOrDefault();
			//hack Чтобы мета свойства объекта не наложились на захардкоренные свойства
			if (maxId < 100000)
				maxId = 100000;
			return _.Tuple(BoxTableNames.ObjectTable, maxId);
		}

		public static Tuple<string, int> LoadPropertyMaxId(BoxDbContext dbContext)
		{
			int maxId = dbContext.Properties.Max(prop => (int?)prop.PropertyId) ?? 0;
			//int maxId = dbContext.Properties.OrderByDescending(prop => prop.PropertyId).Select(prop => prop.PropertyId).Take(1).FirstOrDefault();
			return _.Tuple(BoxTableNames.PropertyTable, maxId);
		}

		public static Tuple<string, int> LoadLinkMaxId(BoxDbContext dbContext)
		{
			int maxId = dbContext.Links.Max(link => (int?)link.LinkId) ?? 0;
			//int maxId = dbContext.Links.OrderByDescending(link => link.LinkId).Select(link => link.LinkId).Take(1).FirstOrDefault();
			return _.Tuple(BoxTableNames.LinkTable, maxId);
		}

		public static ObjectBox LoadOrCreateObjects(DataLayer dbConnection, BoxDbContext dbContext, 
			int typeId,	Func<string, string> jsonIdGetter, params string[] kinds)
		{
			ObjectBox box = new(dbConnection, dbContext, db => db.Objects.ForType(typeId));

			bool isCreated = false;
			foreach (string kind in kinds)
			{
				string jsonId = jsonIdGetter(kind);
				if (box.ObjectsByJsonId.Rows(jsonId).Length == 0)
				{
					int createId = box.CreateAndAddObject(typeId, jsonId, null);
					isCreated = true;
				}
			}

			if (isCreated)
			{
				box.Update(dbContext, true);
			}

			return box;
		}

		//public static LightObject LoadOrCreateObject(DataLayer dbConnection, BoxDbContext dbContext, int typeId,
		//	Func<string, string> jsonIdGetter, string kind)
		//{
		//	ObjectBox box = LoadOrCreateObjects(dbConnection, dbContext, typeId, jsonIdGetter, kind);
		//	return new LightObject(box, box.AllObjectIds[0]);
		//}

		public static LightObject LoadOrCreateObject(DataLayer dbConnection, BoxDbContext dbContext, int typeId, string jsonId)
		{
			ObjectBox box = new(dbConnection, dbContext, db => db.Objects.Where(obj => obj.TypeId == typeId && obj.JsonId == jsonId));

			int[] allObjectIds = box.AllObjectIds;

			//Log.Information("LoadOrCreateObject: {0}, '{1}', {2}", typeId, jsonId, allObjectIds.Length);

			if (allObjectIds.Length != 0)
				return new LightObject(box, allObjectIds[0]);

			int objectId = box.CreateAndAddObject(typeId, jsonId, DateTime.UtcNow);

			box.Update(dbContext, true);

			return new LightObject(box, objectId);
		}


		public static LightObject? LoadObject(DataLayer dbConnection, BoxDbContext dbContext, int typeId, string jsonId)
		{
			ObjectBox box = new(dbConnection, dbContext, db => db.Objects.Where(obj => obj.TypeId == typeId && obj.JsonId == jsonId)); 

			int[] allObjectIds = box.AllObjectIds;
			if (allObjectIds.Length == 0)
				return null;
			return new LightObject(box, allObjectIds[0]);
		}

		public static LightObject? LoadObject(DataLayer dbConnection, BoxDbContext dbContext, int objectId)
		{
			ObjectBox box = new(dbConnection, dbContext, db => db.Objects.ForObjects(objectId));
			int[] allObjectIds = box.AllObjectIds;
			if (allObjectIds.Length == 0)
				return null;
			return new LightObject(box, allObjectIds[0]);
		}

		public static LightParent? LoadParent(DataLayer dbConnection, BoxDbContext dbContext, int objectId)
		{
			ParentBox box = new(dbConnection, dbContext, db => db.Objects.ForObjects(objectId));
			int[] allObjectIds = box.AllObjectIds;
			if (allObjectIds.Length == 0)
				return null;
			return new LightParent(box, allObjectIds[0]);
		}

		public static LightKin? LoadKin(DataLayer dbConnection, BoxDbContext dbContext, int objectId)
		{
			KinBox box = new(dbConnection, dbContext, db => db.Objects.ForObjects(objectId));
			int[] allObjectIds = box.AllObjectIds;
			if (allObjectIds.Length == 0)
				return null;
			return new LightKin(box, allObjectIds[0]);
		}

		//public static RowLink CreateAndFillLinkRow(TableLink linkTableLink, int parentId, int linkTypeId,
		//	int linkIndex, int childId)
		//{
		//	RowLink newLinkRow = linkTableLink.NewRow();
		//	LinkType.ParentId.Set(newLinkRow, parentId);
		//	LinkType.TypeId.Set(newLinkRow, linkTypeId);
		//	LinkType.LinkIndex.Set(newLinkRow, linkIndex);
		//	LinkType.ChildId.Set(newLinkRow, childId);
		//	return newLinkRow;
		//}

		//public static int NewPropertyIndex<T>(IndexLink<T> propertyLink, T propertyKind,
		//	FieldBlank<int> indexBlank, int objectId)
		//{
		//	RowLink[] rows = propertyLink.Rows(propertyKind, objectId);
		//	if (rows.Length != 0)
		//		return indexBlank.Get(_.Last(rows)) + 1;
		//	return 0;
		//}

		public static void RemoveParentObjectWithChilds(BoxDbContext dbContext, int objectId)
		{
			RemoveRecursiveChildObject(dbContext, objectId);

			LinkRow[] parentLinks = dbContext.Links.Where(link => link.ChildId == objectId).ToArray();
			foreach (LinkRow row in parentLinks)
				dbContext.Links.Remove(row);
		}

		static void RemoveRecursiveChildObject(BoxDbContext dbContext, int objectId)
		{
			LinkRow[] links = dbContext.Links.Where(link => link.ParentId == objectId).ToArray();

			foreach (LinkRow link in links)
				RemoveRecursiveChildObject(dbContext, link.ChildId);

			foreach (LinkRow link in links)
				dbContext.Links.Remove(link);

			PropertyRow[] properties = dbContext.Properties.Where(property => property.ObjectId == objectId).ToArray();
			foreach (PropertyRow property in properties)
				dbContext.Properties.Remove(property);

			ObjectRow[] objects = dbContext.Objects.Where(obj => obj.ObjectId == objectId).ToArray();
			foreach (ObjectRow obj in objects)
				dbContext.Objects.Remove(obj);
		}

		public static void RemoveKinObject(KinBox kinBox, int objectId)
		{
			foreach (LinkRow link in kinBox.ParentsByObjectId.Rows(objectId))
			{
				kinBox.ParentsByObjectId.TableLink.RemoveRow(link);
			}
			RemoveParentObject(kinBox, objectId);
		}

		public static void RemoveParentObject(ParentBox parentBox, int objectId)
		{
			foreach (LinkRow link in parentBox.ChildsByObjectId.Rows(objectId))
			{
				parentBox.ChildsByObjectId.TableLink.RemoveRow(link);
			}

			RemoveVacantObject(parentBox, objectId);
		}

		public static void RemoveVacantObject(ObjectBox objectBox, int objectId)
		{
			foreach (PropertyRow property in objectBox.PropertiesByObjectId.Rows(objectId))
			{
				objectBox.PropertiesByObjectId.TableLink.RemoveRow(property);
			}
			ObjectRow? objectRow = objectBox.ObjectById.Row(objectId);
			if (objectRow != null)
				objectBox.ObjectById.TableLink.RemoveRow(objectRow);
		}

		//public static PropertyBlank<TField> Create<TField>(int propertyKind, FieldBlank<PropertyRow, TField> field)
		//{
		//	return new PropertyBlank<TField>(propertyKind, field);
		//}

		public readonly static FieldBlank<PropertyRow, string> StringValue = new("",
			property => property.PropertyValue ?? "", 
			(property, value) => property.PropertyValue = value
		);

		public readonly static FieldBlank<PropertyRow, int> IntValue = new(0,
			property => ConvertHlp.ToInt(property.PropertyValue) ?? 0,
			(property, value) => property.PropertyValue = ConvertHlp.FromInt(value)
		);

		public readonly static FieldBlank<PropertyRow, int?> IntNullableValue = new(null,
			property => ConvertHlp.ToInt(property.PropertyValue),
			(property, value) => property.PropertyValue = ConvertHlp.FromInt(value)
		);

		public readonly static FieldBlank<PropertyRow, float> FloatValue = new(0f,
			property => ConvertHlp.ToFloat(property.PropertyValue) ?? 0,
			(property, value) => property.PropertyValue = ConvertHlp.FromFloat(value)
		);

		public readonly static FieldBlank<PropertyRow, double?> DoubleNullableValue = new(null,
			property => ConvertHlp.ToDouble(property.PropertyValue),
			(property, value) => property.PropertyValue = ConvertHlp.FromDouble(value)
		);

		public readonly static FieldBlank<PropertyRow, double> DoubleValue = new(0,
			property => ConvertHlp.ToDouble(property.PropertyValue) ?? 0,
			(property, value) => property.PropertyValue = ConvertHlp.FromDouble(value)
		);

		public readonly static FieldBlank<PropertyRow, bool> BoolValue = new(false,
			property => ConvertHlp.ToBool(property.PropertyValue) ?? false,
			(property, value) => property.PropertyValue = ConvertHlp.FromBool(value)
		);

		public readonly static FieldBlank<PropertyRow, DateTime?> DateTimeNullableValue = new(null,
			property => ConvertHlp.TicksToDateTime(property.PropertyValue),
			(property, value) => property.PropertyValue = ConvertHlp.TicksFromDateTime(value)
		);

		//public static LinkRow CreateAndFillLinkRow(TableLink<LinkRow> linkTableLink, int parentId, int linkTypeId,
		//	int linkIndex, int childId)
		//{
		//	RowLink newLinkRow = linkTableLink.NewRow();
		//	LinkType.ParentId.Set(newLinkRow, parentId);
		//	LinkType.TypeId.Set(newLinkRow, linkTypeId);
		//	LinkType.LinkIndex.Set(newLinkRow, linkIndex);
		//	LinkType.ChildId.Set(newLinkRow, childId);
		//	return newLinkRow;
		//}


	}
}
