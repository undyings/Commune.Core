using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class FieldBlank<T, TField>
		where T : class
	{
		public readonly TField DefaultValue;
		readonly Func<T, TField> valueGetter;
		readonly Action<T, TField> valueSetter;

		public FieldBlank(TField defaultValue, Func<T, TField> valueGetter, Action<T, TField> valueSetter)
		{
			this.DefaultValue = defaultValue;
			this.valueGetter = valueGetter;
			this.valueSetter = valueSetter;
		}

		public TField GetValue(T row)
		{
			return valueGetter(row);
		}

		public void SetValue(T row, TField value)
		{
			valueSetter(row, value);
		}
	}
}
