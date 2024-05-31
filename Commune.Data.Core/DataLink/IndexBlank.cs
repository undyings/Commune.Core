using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public abstract class IndexBlank<T>
		where T : class
	{
		public readonly string IndexName;
		public readonly bool IsMultiIndex;
		readonly Func<T, UniversalKey> keyGetter;

		public UniversalKey CreateKey(T row)
		{
			return keyGetter(row);
		}

		protected IndexBlank(bool isMultiIndex, string indexName, Func<T, UniversalKey> keyGetter)
		{
			this.IsMultiIndex = isMultiIndex;
			this.IndexName = indexName;
			this.keyGetter = keyGetter;
		}
	}

	public class SingleIndexBlank<T> : IndexBlank<T>
		where T : class
	{
		public SingleIndexBlank(string indexName, Func<T, UniversalKey> keyGetter) :
			base(false, indexName, keyGetter)
		{
		}
	}

	public class MultiIndexBlank<T> : IndexBlank<T>
		where T : class
	{
		public MultiIndexBlank(string indexName, Func<T, UniversalKey> keyGetter) :
			base(true, indexName, keyGetter)
		{
		}
	}

	public class OrderedIndexBlank<T> : MultiIndexBlank<T>
		where T : class
	{
		public readonly FieldBlank<T, int> OrderField;

		public OrderedIndexBlank(string indexName, Func<T, UniversalKey> keyGetter, FieldBlank<T, int> orderField) :
			base(indexName, keyGetter)
		{
			this.OrderField = orderField;
		}
	}
}
