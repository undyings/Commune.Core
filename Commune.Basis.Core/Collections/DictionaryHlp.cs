using System;
using System.Collections.Generic;
using System.Text;

namespace Commune.Basis
{
  public static class DictionaryHlp
  {
    public static TValue? Find<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey? key)
    {
      if (key == null)
        return default(TValue?);

      TValue? value;
      if (dict.TryGetValue(key, out value))
        return value;
      return default(TValue?);
    }

		public static void AddValue<TKey, TValue>(this IDictionary<TKey, List<TValue>> index, TKey key, TValue value)
		{
			List<TValue>? values;
			if (!index.TryGetValue(key, out values))
			{
				values = new List<TValue>();
				index[key] = values;
			}
			values.Add(value);
		}


		public static Dictionary<TKey, List<TItem>> ToMultiDictionary<TItem, TKey>(
			this IEnumerable<TItem> items, Func<TItem, TKey> keyGetter) where TKey : notnull
		{
			Dictionary<TKey, List<TItem>> valuesByKey = new Dictionary<TKey, List<TItem>>();
			foreach (TItem item in items)
			{
				TKey key = keyGetter(item);

				List<TItem>? values;
				if (!valuesByKey.TryGetValue(key, out values))
				{
					values = new List<TItem>();
					valuesByKey[key] = values;
				}

				values.Add(item);
			}

			return valuesByKey;
		}

		public static Dictionary<TKey, List<TValue>> ToMultiDictionary<TItem, TKey, TValue>(
			this IEnumerable<TItem> items, Func<TItem, TKey> keyGetter, Func<TItem, TValue> valueGetter) where TKey : notnull
		{
			Dictionary<TKey, List<TValue>> valuesByKey = new Dictionary<TKey, List<TValue>>();
			foreach (TItem item in items)
			{
				TKey key = keyGetter(item);
				TValue value = valueGetter(item);

				List<TValue>? values;
				if (!valuesByKey.TryGetValue(key, out values))
				{
					values = new List<TValue>();
					valuesByKey[key] = values;
				}

				values.Add(value);
			}

			return valuesByKey;
		}

		public static TValue[] FindAll<TKey, TValue>(
			this Dictionary<TKey, TValue> valueByKey, IEnumerable<TKey> keys) where TKey : notnull
		{
			if (keys == null)
				return new TValue[0];

			List<TValue> values = new List<TValue>();
			foreach (TKey key in keys)
			{
				TValue? value;
				if (valueByKey.TryGetValue(key, out value))
					values.Add(value);
			}

			return values.ToArray();
		}


	}
}
