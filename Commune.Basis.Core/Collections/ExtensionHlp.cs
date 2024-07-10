using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Basis
{
	public static class ExtensionHlp
	{
		public static IEnumerable<T> Prepend2<T>(this IEnumerable<T> items, T? firstItem)
		{
			if (firstItem != null)
				yield return firstItem;

			foreach (T item in items)
				yield return item;
		}

		public static IEnumerable<T> Prepend2<T>(this IEnumerable<T> items, params T?[] beginItems)
		{
			foreach (T? item in beginItems)
			{
				if (item != null)
					yield return item;
			}

			foreach (T item in items)
				yield return item;
		}

		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> items, IEnumerable<T> beginItems)
		{
			foreach (T item in beginItems)
				yield return item;

			foreach (T item in items)
				yield return item;
		}

		public static IEnumerable<T> ExcludeNull<T>(this IEnumerable<T?> items)
		{
			foreach (T? item in items)
			{
				if (item != null)
					yield return item;
			}
		}
	}
}
