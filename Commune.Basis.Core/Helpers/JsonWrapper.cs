using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Commune.Basis
{
	public abstract class IJwElement
	{
		public abstract void ToJson(StringBuilder sb);

		public override string ToString()
		{
			StringBuilder sb = new();
			ToJson(sb);

			return sb.ToString();
		}
	}

	public class JwCode : IJwElement
	{
		readonly string Code;
		public JwCode(string code)
		{
			Code = code;
		}

		public override void ToJson(StringBuilder sb)
		{
			sb.Append(Code);
		}
	}

	public class JwValue : IJwElement
	{
		readonly string Value;
		readonly bool WithQuote;
		public JwValue(string value, bool withQuote)
		{
			this.Value = value;
			this.WithQuote = withQuote;
		}

		public JwValue(int value) : this(value.ToString(), false)
		{
		}

		public JwValue(bool value) : this(value ? "true" : "false", false)
		{
		}

		public JwValue(string value) : this(value, true)
		{
		}


		public override void ToJson(StringBuilder sb)
		{
			if (WithQuote)
				sb.Append('"');
			sb.Append(JavaScriptEncoder.Default.Encode(Value));
			//sb.Append(Value);
			if (WithQuote)
				sb.Append('"');
		}
	}

	public class JwField : IJwElement
	{
		readonly string Name;
		readonly IJwElement Value;
		public JwField(string name, IJwElement value)
		{
			this.Name = name;
			this.Value = value;
		}

		public override void ToJson(StringBuilder sb)
		{
			sb.Append(Name);
			sb.Append(':');
			Value.ToJson(sb);
		}
	}

	public class JwObject : IJwElement
	{
		readonly JwField[] Fields;
		public JwObject(params JwField[] fields)
		{
			this.Fields = fields;
		}

		public override void ToJson(StringBuilder sb)
		{
			sb.Append('{');
			int i = -1;
			foreach (JwField field in Fields)
			{
				++i;
				if (i > 0)
					sb.Append(",");
				field.ToJson(sb);
			}
			sb.Append('}');
		}
	}

	public class JwArray : IJwElement
	{
		readonly IEnumerable<IJwElement> Items;

		public JwArray(IEnumerable<JwObject> objects)
		{
			this.Items = objects;
		}

		public JwArray(params JwObject[] objects)
		{
			this.Items = objects;
		}

		public JwArray(IEnumerable<JwValue> values)
		{
			this.Items = values;
		}

		public JwArray(params JwValue[] values)
		{
			this.Items = values;
		}

		public JwArray(IEnumerable<JwArray> arrays)
		{
			this.Items = arrays;
		}

		public JwArray(string key, IJwElement value)
		{
			this.Items = new IJwElement[] { new JwValue(key), value };
		}

		public override void ToJson(StringBuilder sb)
		{
			sb.Append('[');
			int i = -1;
			foreach (IJwElement item in Items)
			{
				++i;
				if (i > 0)
					sb.Append(",");
				item.ToJson(sb);
			}
			sb.Append(']');
		}
	}
}
