using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Commune.Data
{
	public class ObjectType
	{
		public readonly static FieldBlank<ObjectRow, string> JsonIdField = new("", obj => obj.JsonId, (obj, jsonIds) => obj.JsonId = jsonIds);

		public readonly static SingleIndexBlank<ObjectRow> ObjectById = new ("ObjectById", obj => new UniversalKey(obj.ObjectId));
		public readonly static MultiIndexBlank<ObjectRow> ObjectsByTypeId = new ("ObjectsByTypeId", obj => new UniversalKey(obj.TypeId));
		public readonly static MultiIndexBlank<ObjectRow> ObjectsByJsonIds = new ("ObjectsByJsonIds", obj => new UniversalKey(obj.JsonId));

		public static void SyncChanges(ObjectRow old, ObjectRow actual)
		{
			if (old.JsonId != actual.JsonId)
				old.JsonId = actual.JsonId;
			if (old.ActFrom != actual.ActFrom)
				old.ActFrom = actual.ActFrom;
			if (old.ActTo != actual.ActTo)
				old.ActTo = actual.ActTo;
		}
	}

	public class PropertyType
	{
		public readonly static FieldBlank<PropertyRow, int> OrderField = new(0, property => property.PropertyIndex, (property, index) => property.PropertyIndex = index);

		public readonly static SingleIndexBlank<PropertyRow> PropertyById = new ("PropertyById", p => new UniversalKey(p.PropertyId));
		public readonly static SingleIndexBlank<PropertyRow> PropertyByUnique = new ("PropertyByUnique", p => new UniversalKey(p.ObjectId, p.TypeId, p.PropertyIndex));
		public readonly static OrderedIndexBlank<PropertyRow> PropertiesByObjectIdAndTypeId = new("PropertiesByObjectIdAndTypeId", 
			property => new UniversalKey(property.ObjectId, property.TypeId), OrderField
		);
		public readonly static MultiIndexBlank<PropertyRow> PropertiesByTypeId = new ("PropertiesByTypeId", p => new UniversalKey(p.TypeId));
		public readonly static MultiIndexBlank<PropertyRow> PropertiesByObjectId = new ("PropertiesByObjectId", p => new UniversalKey(p.ObjectId));

		public static void SyncChanges(PropertyRow old, PropertyRow actual)
		{
			if (old.PropertyIndex != actual.PropertyIndex)
				old.PropertyIndex = actual.PropertyIndex;
			if (old.PropertyValue != actual.PropertyValue)
				old.PropertyValue = actual.PropertyValue;
		}
	}

	public class LinkType
	{
		public readonly static FieldBlank<LinkRow, int> OrderField = new(0, link => link.LinkIndex, (link, index) => link.LinkIndex = index);

		public readonly static SingleIndexBlank<LinkRow> LinkById = new ("LinkById", link => new UniversalKey(link.LinkId));
		public readonly static OrderedIndexBlank<LinkRow> LinksByParentIdAndTypeId = new("LinksByParentIdAndTypeId", 
			link => new UniversalKey(link.ParentId, link.TypeId), OrderField
		);
		public readonly static MultiIndexBlank<LinkRow> LinksByParentId = new ("LinksByParentId", link => new UniversalKey(link.ParentId));
		public readonly static MultiIndexBlank<LinkRow> LinksByChildId = new ("LinksByChildId", link => new UniversalKey(link.ChildId));
		public readonly static OrderedIndexBlank<LinkRow> LinksByChildIdAndTypeId = new("LinksByChildIdAndTypeId", 
			link => new UniversalKey(link.ChildId, link.TypeId), OrderField
		);

		public static void SyncChanges(LinkRow old, LinkRow actual)
		{
			if (old.ParentId != actual.ParentId)
				old.ParentId = actual.ParentId;
			if (old.LinkIndex != actual.LinkIndex)
				old.LinkIndex = actual.LinkIndex;
			if (old.ChildId != actual.ChildId)
				old.ChildId = actual.ChildId;
		}
	}
}
