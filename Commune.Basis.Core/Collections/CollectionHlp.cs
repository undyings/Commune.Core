using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Basis
{
	public class CollectionHlp
	{
		//public static IEnumerable<T> Merge<T>(T? firstItem, IEnumerable<T> items)
		//{
		//	if (firstItem != null)
		//		yield return firstItem;

		//	foreach (T item in items)
		//		yield return item;
		//}

		/// <summary>
		/// оставляем для каждого ключа только первое вхождение
		/// </summary>
		public static Dictionary<TKey, TItem> MakeUniqueIndex<TKey, TItem>(IEnumerable<TItem> items, Func<TItem, TKey> keyGetter)
			where TKey : notnull
		{
			Dictionary<TKey, TItem> index;
			if (items is ICollection<TItem>)
				index = new Dictionary<TKey, TItem>(((ICollection<TItem>)items).Count);
			else
				index = new Dictionary<TKey, TItem>();

			foreach (TItem item in items)
			{
				TKey key = keyGetter(item);
				index[key] = item;
			}
			return index;
		}

		/// <summary>
		/// Возвращает true если длина коллекции больше заданной, иначе - false.
		/// </summary>
		public static bool CountMore(IEnumerable? collection, int specifiedCount)
		{
			if (collection == null)
				return false;

			int count = 0;
			foreach (object item in collection)
			{
				count++;
				if (count > specifiedCount)
					return true;
			}
			return false;
		}

		public static List<TItem> SortBy<TKey, TItem>(IEnumerable<TItem> items, Func<TItem, TKey> getter)
			where TKey : IComparable
		{
			List<TItem> list = new(items);
			list.Sort(delegate (TItem s1, TItem s2) { return getter(s1).CompareTo(getter(s2)); });
			return list;
		}

		public static List<TItem> SortBy<TKey, TItem>(IEnumerable<TItem> items, Func<TItem, TKey> getter,
			Comparison<TKey> comparer)
		{
			List<TItem> list = new(items);
			list.Sort(delegate (TItem s1, TItem s2) { return comparer(getter(s1), getter(s2)); });
			return list;
		}

		/// <summary>
		/// Поэлементно сравнивает содержимое двух массивов одинаковой длины
		/// </summary>
		public static int ArrayComparison<T>(T[] values1, T[] values2)
		{
			int result = values1.Length.CompareTo(values2.Length);
			if (result != 0)
				return result;

			for (int i = 0; i < values1.Length; ++i)
			{
				int cmp = ValueComparison(values1[i], values2[i]);
				if (cmp != 0)
					return cmp;
			}
			return 0;
		}

		/// <summary>
		/// Сравнивает два значения. Если ни одно из значений не поддерживает IComparable возвращает 0.
		/// </summary>
		public static int ValueComparison(object? value1, object? value2)
		{
			IComparable? comparable1 = value1 as IComparable;
			if (comparable1 != null)
			{
				return comparable1.CompareTo(value2);
			}
			else
			{
				IComparable? comparable2 = value2 as IComparable;
				if (comparable2 != null)
				{
					return -comparable2.CompareTo(value1);
				}
			}
			return 0;
		}


		public static void Synchronize<TLeft, TRight, TKey>(
			IEnumerable<TLeft> left, Func<TLeft, TKey> leftKeyer,
			IEnumerable<TRight> right, Func<TRight, TKey> rightKeyer,
			Action<TLeft> onlyLeft, Action<TRight> onlyRight, Action<TLeft, TRight> both)
			where TKey : notnull
		{
			Dictionary<TKey, TRight> rightIndex = CollectionHlp.MakeUniqueIndex(right, rightKeyer);

			foreach (TLeft l in left)
			{
				TKey lkey = leftKeyer(l);
				TRight? r;
				if (rightIndex.TryGetValue(lkey, out r))
				{
					both(l, r);
					rightIndex.Remove(lkey);
				}
				else
				{
					onlyLeft(l);
				}
			}

			foreach (KeyValuePair<TKey, TRight> rPair in rightIndex)
				onlyRight(rPair.Value);
		}

		public static List<IntervalUnion<T>> UniteOverlapIntervals<T>(IList<T> intervals,
			Func<T, DateTime[]> intervalGetter)
		{
			List<IntervalUnion<T>> unions = new List<IntervalUnion<T>>();
			if (intervals.Count == 0)
				return unions;

			List<T> sources = new List<T>();
			sources.Add(intervals[0]);
			DateTime[] unionInterval = intervalGetter(intervals[0]);
			for (int i = 1; i < intervals.Count; ++i)
			{
				DateTime[] interval = intervalGetter(intervals[i]);
				if (interval[0] > unionInterval[1])
				{
					unions.Add(new IntervalUnion<T>(unionInterval[0], unionInterval[1], sources.ToArray()));
					sources.Clear();
					sources.Add(intervals[i]);
					unionInterval = interval;
					continue;
				}

				sources.Add(intervals[i]);
				if (interval[1] > unionInterval[1])
					unionInterval[1] = interval[1];
			}

			unions.Add(new IntervalUnion<T>(unionInterval[0], unionInterval[1], sources.ToArray()));
			return unions;
		}

		public static int BinarySearch<TKey, TItem>(TItem searchItem, IList<TItem> collection,
			Func<TItem, TKey> keyGetter, Func<TKey, TKey, int> comparer)
		{
			return BinarySearch(collection, keyGetter(searchItem), keyGetter, comparer);
		}

		public static int BinarySearch<TKey, TItem>(IList<TItem> collection, TKey searchKey,
			Func<TItem, TKey> keyGetter)
		{
			return BinarySearch(collection, searchKey, keyGetter, Comparer<TKey>.Default.Compare);
		}

		public static int BinarySearch<TKey, TItem>(IList<TItem> collection, TKey searchKey,
			Func<TItem, TKey> keyGetter, Func<TKey, TKey, int> comparer)
		{
			if (comparer == null)
				comparer = Comparer<TKey>.Default.Compare;

			int beginIndex = 0;
			int endIndex = collection.Count - 1;
			while (beginIndex <= endIndex)
			{
				int bisectionIndex = beginIndex + ((endIndex - beginIndex) >> 1);
				int result = comparer(keyGetter(collection[bisectionIndex]), searchKey);
				if (result == 0)
				{
					return bisectionIndex;
				}
				if (result < 0)
				{
					beginIndex = bisectionIndex + 1;
				}
				else
				{
					endIndex = bisectionIndex - 1;
				}
			}
			return ~beginIndex;
		}

		public static bool InsertInSortedList<TItem, TKey>(IList<TItem> collection, TItem insertItem,
			Func<TItem, TKey> keyGetter)
		{
			return InsertInSortedList(collection, insertItem, keyGetter, Comparer<TKey>.Default.Compare, false);
		}

		public static bool InsertInSortedList<TItem, TKey>(IList<TItem> collection, TItem insertItem,
			Func<TItem, TKey> keyGetter, Func<TKey, TKey, int> comparer, bool disableRepeats)
		{
			int position = BinarySearch(insertItem, collection, keyGetter, comparer);
			if (position < 0)
			{
				if (disableRepeats)
					return false;
				position = ~position;
			}
			collection.Insert(position, insertItem);
			return true;
		}
	}
}
