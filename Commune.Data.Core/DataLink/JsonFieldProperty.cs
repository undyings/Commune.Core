using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web;
using Commune.Basis;
using System.Text.Json;

namespace Commune.Data
{
	public class JsonUniqueBlank<TField> : JsonFieldBlank<ObjectRow, TField>
		 where TField : new()
	{
		public JsonUniqueBlank() :
			base(ObjectType.JsonIdField)
		{
		}

		public bool SetWithCheck(BoxDbContext context, ObjectHeadBox objectBox, ObjectRow row, TField field)
		{
			UniqueChecker uniqueChecker = objectBox.ObjectUniqueChecker;

			string jsonIds = Create(field);

			if (!uniqueChecker.IsUniqueKey(context, row.ObjectId, row.TypeId, jsonIds, row.ActFrom))
				return false;

			row.JsonId = jsonIds;
			return true;
		}

	}

	public class JsonFieldBlank<TRow, TField>
		where TRow : class
		where TField : new()
	{
		public readonly FieldBlank<TRow, string> FieldBlank;

		public JsonFieldBlank(FieldBlank<TRow, string> fieldBlank)
		{
			this.FieldBlank = fieldBlank;
		}

		public TField Get(TRow row)
		{
			string jsonString = FieldBlank.GetValue(row);

			try
			{
				return JsonSerializer.Deserialize<TField>(jsonString, JsonHlp.WithIncludeFields) ?? new TField();
			}
			catch
			{
				return new TField();
			}
		}

		public string Create(TField field)
		{
			return JsonSerializer.Serialize(field, JsonHlp.WithIncludeFields);
		}

		public void SetWithoutCheck(TRow row, TField field)
		{
			string jsonStr = JsonSerializer.Serialize(field, JsonHlp.WithIncludeFields);

			FieldBlank.SetValue(row, jsonStr);
		}
	}
}
