using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Commune.Basis;

namespace Commune.Data
{
	public class LightHead
	{
		readonly ObjectHeadBox headBox;
		public readonly int Id;
		public LightHead(ObjectHeadBox objectBox, int objectId)
		{
			this.headBox = objectBox;
			this.Id = objectId;
		}

		public bool IsKind(int objectKind)
		{
			return headBox.IsKindObject(Id, objectKind);
		}

		public ObjectRow Head
		{
			get
			{
				//hack Логически возвращение пустого объекта невозможно. Возвращаем пустой объект, чтобы избежать проверок на null
				return headBox.ObjectById.Row(Id) ?? new ObjectRow(); 
			}
		}

		public TField Get<TField>(JsonUniqueBlank<TField> property) where TField : new()
		{
			return property.Get(Head);
		}

		public void SetCreateTime()
		{
			DateTime utcTime = DateTime.UtcNow;
			this.Head.ActFrom = utcTime;
			this.Head.ActTo = utcTime;
		}

	}

	public class LightObject : LightHead
	{
		readonly ObjectBox objectBox;
		public LightObject(ObjectBox objectBox, int objectId) :
			base(objectBox, objectId)
		{
			this.objectBox = objectBox;
		}

		public ObjectBox Box
		{
			get { return objectBox; }
		}

		//public int NewPropertyIndex(int propertyKind)
		//{
		//	PropertyRow[] rows = Box.PropertiesByObjectIdWithKind.Rows(propertyKind, Id);
		//	if (rows.Length != 0)
		//		return rows.Last().PropertyIndex + 1;
		//	return 0;
		//}

		public TField Get<TField>(IPropertyBlank<PropertyRow, TField> property, int propertyIndex)
		{
			PropertyRow? row = this.Box.PropertiesByObjectIdWithKind.Row(property.Kind, propertyIndex, Id);
			if (row == null)
				return property.Field.DefaultValue;

			return property.Field.GetValue(row);
		}

		public TField Get<TField>(IPropertyBlank<PropertyRow, TField> property)
		{
			return Get(property, 0);
		}


		PropertyRow FindOrCreateRow(int propertyKind, int propertyIndex)
		{
			PropertyRow? row = objectBox.PropertiesByObjectIdWithKind.Row(propertyKind, propertyIndex, Id);
			if (row != null)
				return row;

			PropertyRow newRow = objectBox.CreatePropertyRow(Id, propertyKind, propertyIndex);

			objectBox.PropertiesByObjectIdWithKind.InsertInArray(newRow, propertyIndex);
			return newRow;
		}

		public void Set<TField>(IPropertyBlank<PropertyRow, TField> property, int propertyIndex, TField propertyValue)
		{
			PropertyRow row = FindOrCreateRow(property.Kind, propertyIndex);
			property.Field.SetValue(row, propertyValue);
		}

		public void Set<TField>(IPropertyBlank<PropertyRow, TField> property, TField propertyValue)
		{
			Set(property, 0, propertyValue);
		}

		public PropertyRow[] AllPropertyRows(IPropertyBlank<PropertyRow> property)
		{
			return objectBox.PropertiesByObjectIdWithKind.Rows(property.Kind, Id);
		}

		public bool RemoveProperty(IPropertyBlank<PropertyRow> property, int propertyIndex)
		{
			return objectBox.PropertiesByObjectIdWithKind.RemoveFromArray(property, propertyIndex, Id);
		}

		public void InsertProperty<TField>(IPropertyBlank<PropertyRow, TField> property, int propertyIndex, TField propertyValue)
		{
			PropertyRow newRow = CreatePropertyRow(property, propertyValue);

			objectBox.PropertiesByObjectIdWithKind.InsertInArray(newRow, propertyIndex);
		}

		public void AddProperty<TField>(IPropertyBlank<PropertyRow, TField> property, TField propertyValue)
		{
			PropertyRow newRow = CreatePropertyRow(property, propertyValue);

			objectBox.PropertiesByObjectIdWithKind.AddInArray(newRow);
		}

		PropertyRow CreatePropertyRow<TField>(IPropertyBlank<PropertyRow, TField> property, TField propertyValue)
		{
			PropertyRow newRow = new()
			{
				ObjectId = Id,
				TypeId = property.Kind
			};

			property.Field.SetValue(newRow, propertyValue);

			return newRow;
		}
	}

	public class LightParent : LightObject
	{
		readonly ParentBox objectBox;
		public LightParent(ParentBox objectBox, int objectId) :
			base(objectBox, objectId)
		{
			this.objectBox = objectBox;
		}

		public LinkRow[] AllChildRows(LinkBlank childLinkKind)
		{
			return objectBox.ChildsByObjectIdWithKind.Rows(childLinkKind.Kind, Id);
		}

		public int? GetChildId(LinkBlank childLinkKind, int linkIndex)
		{
			LinkRow? row = FindChildRow(childLinkKind, linkIndex);
			if (row == null)
				return null;
			return row.ChildId;
		}

		public int? GetChildId(LinkBlank childLinkKind)
		{
			return GetChildId(childLinkKind, 0);
		}

		LinkRow? FindChildRow(LinkBlank childLinkKind, int linkIndex)
		{
			LinkRow[] rows = objectBox.ChildsByObjectIdWithKind.Rows(childLinkKind.Kind, Id);
			int position = _.BinarySearch(rows, linkIndex,
				delegate (LinkRow row) { return row.LinkIndex; }, Comparer<int>.Default.Compare);
			if (position < 0)
				return null;
			return rows[position];
		}

		public void SetChildId(LinkBlank childLinkKind, int linkIndex, int? childId)
		{
			LinkRow? row = FindChildRow(childLinkKind, linkIndex);
			if (childId == null)
			{
				if (row != null)
					objectBox.ChildByLinkId.TableLink.RemoveRow(row);
				return;
			}

			if (row != null)
			{
				row.ChildId = childId.Value;
			}
			else if (row == null)
			{
				LinkRow newRow = objectBox.CreateLinkRow(Id, childLinkKind.Kind, linkIndex, childId.Value);
				objectBox.ChildByLinkId.TableLink.AddRow(newRow);
				//objectBox.ChildsByObjectIdWithKind.InsertInArray(newRow, linkIndex);
			}
		}

		public void SetChildId(LinkBlank childLinkKind, int? childId)
		{
			SetChildId(childLinkKind, 0, childId);
		}


		public int[] AllChildIds(LinkBlank childLinkKind)
		{
			return AllChildRows(childLinkKind).Select(link => link.ChildId).ToArray();
		}

		public bool RemoveChildLink(LinkBlank childLinkKind, int linkIndex)
		{
			return objectBox.ChildsByObjectIdWithKind.RemoveFromArray(childLinkKind, linkIndex, Id);
		}

		public LinkRow InsertChildLink(LinkBlank childLinkKind, int linkIndex, int childId)
		{
			LinkRow newRow = objectBox.CreateLinkRow(Id, childLinkKind.Kind, linkIndex, childId);

			objectBox.ChildsByObjectIdWithKind.InsertInArray(newRow, linkIndex);
			return newRow;
		}

		public LinkRow AddChildLink(LinkBlank childLinkKind, int childId)
		{
			LinkRow newRow = objectBox.CreateLinkRow(Id, childLinkKind.Kind, 0, childId);

			objectBox.ChildsByObjectIdWithKind.AddInArray(newRow);
			return newRow;
		}
	}

	public class LightKin : LightParent
	{
		readonly KinBox objectBox;
		public LightKin(KinBox objectBox, int objectId) :
			base(objectBox, objectId)
		{
			this.objectBox = objectBox;
		}

		public int? GetParentId(LinkBlank parentLinkKind, int linkIndex)
		{
			LinkRow? row = FindParentRow(parentLinkKind, linkIndex);
			if (row == null)
				return null;
			return row.ParentId;
		}

		public int? GetParentId(LinkBlank parentLinkKind)
		{
			return GetParentId(parentLinkKind, 0);
		}

		//public void AddParentId(LinkBlank parentLinkKind, int parentId)
		//{
		//	LinkRow[] rows = objectBox.ParentsByObjectIdWithKind.Rows(parentLinkKind.Kind, Id);

		//	SetParentId(parentLinkKind, rows.Length, parentId);
		//}

		//public void SetParentId(LinkBlank parentLinkKind, int linkIndex, int? parentId)
		//{
		//	LinkRow? row = FindParentRow(parentLinkKind, linkIndex);
		//	if (parentId == null)
		//	{
		//		if (row != null)
		//			objectBox.ParentTable.RemoveRow(row);
		//		return;
		//	}

		//	if (row != null)
		//	{
		//		row.ParentId = parentId.Value;
		//	}
		//	else if (row == null)
		//	{
		//		LinkRow newRow = objectBox.CreateLinkRow(parentId.Value, parentLinkKind.Kind, linkIndex, Id);

		//		objectBox.ParentTable.AddRow(newRow);
		//		objectBox.ParentTable.CreateIndexForRow(newRow);
		//	}
		//}

		//public void SetParentId(LinkBlank parentLinkKind, int parentId)
		//{
		//	SetParentId(parentLinkKind, 0, parentId);
		//}

		//public void InsertParentId(LinkBlank parentLinkKind, int linkIndex, int parentId)
		//{
		//	RowLink row = DataBox.CreateAndFillLinkRow(objectBox.ParentsByObjectIdWithKind.TableLink,
		//		parentId, parentLinkKind.Kind, linkIndex, Id);
		//	objectBox.ParentsByObjectIdWithKind.InsertOver(row);
		//}

		public LinkRow[] AllParentRows(LinkBlank parentLinkKind)
		{
			return objectBox.ParentsByObjectIdWithKind.Rows(parentLinkKind.Kind, Id);
		}

		LinkRow? FindParentRow(LinkBlank parentLinkKind, int linkIndex)
		{
			LinkRow[] rows = objectBox.ParentsByObjectIdWithKind.Rows(parentLinkKind.Kind, Id);
			int position = _.BinarySearch(rows, linkIndex, link => link.LinkIndex);
			if (position < 0)
				return null;
			return rows[position];
		}
	}



}
