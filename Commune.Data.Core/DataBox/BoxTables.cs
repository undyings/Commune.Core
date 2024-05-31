using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	[Table(BoxTableNames.ObjectTable)]
	[Index(nameof(TypeId), nameof(ActFrom))]
	[Index(nameof(TypeId), nameof(ActTo))]
	[Index(nameof(TypeId), nameof(JsonId))]
	public class ObjectRow
	{
		[Key]
		[Column("obj_id")]
		public int ObjectId { get; set; }
		[Column("type_id")]
		public int TypeId { get; set; }
		[Column("json_ids")]
		public string JsonId { get; set; }
		[Column("act_from")]
		public DateTime? ActFrom { get; set; }
		[Column("act_to")]
		public DateTime? ActTo { get; set; }

		public ObjectRow()
		{
			this.JsonId = "";
		}
	}

	[Table(BoxTableNames.PropertyTable)]
	[Index(nameof(ObjectId), nameof(TypeId), nameof(PropertyIndex), IsUnique = true)]
	[Index(nameof(TypeId))]
	public class PropertyRow
	{
		[Key]
		[Column("prop_id")]
		public int PropertyId { get; set; }
		[Column("obj_id")]
		public int ObjectId { get; set; }
		[Column("type_id")]
		public int TypeId { get; set; }
		[Column("prop_index")]
		public int PropertyIndex { get; set; }
		[Column("prop_value")]
		public string? PropertyValue { get; set; }
	}

	[Table(BoxTableNames.LinkTable)]
	[Index(nameof(ParentId), nameof(TypeId), nameof(LinkIndex), IsUnique = true)]
	[Index(nameof(ChildId), nameof(TypeId))]
	public class LinkRow
	{
		[Key]
		[Column("link_id")]
		public int LinkId { get; set; }
		[Column("parent_id")]
		public int ParentId { get; set; }
		[Column("type_id")]
		public int TypeId { get; set; }
		[Column("link_index")]
		public int LinkIndex { get; set; }
		[Column("child_id")]
		public int ChildId { get; set; }
	}

	//[Table(BoxTableNames.PrimaryKeyTable)]
	//public class PrimaryKeyRow
	//{
	//	[Key]
	//	[Column("table_name")]
	//	public string TableName { get; set; }
	//	[Column("max_primary_key")]
	//	public int MaxPrimaryKey { get; set; }

	//	public PrimaryKeyRow()
	//	{
	//		this.TableName = "";
	//	}
	//}
}
