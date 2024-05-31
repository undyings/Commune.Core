using Commune.Basis;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class SingleIndexLink<T>
		where T : class
	{
		public readonly TableLink<T> TableLink;
		public readonly SingleIndexBlank<T> IndexBlank;

		public SingleIndexLink(TableLink<T> tableLink, SingleIndexBlank<T> indexBlank) 
		{
			this.TableLink = tableLink;
			this.IndexBlank = indexBlank;
		}

		public T? Row(params object[] keyParts)
		{
			return TableLink.FindRow(IndexBlank, keyParts);
		}

		public bool Exist(params object[] keyParts)
		{
			return TableLink.FindRow(IndexBlank, keyParts) != null;
		}

		public ICollection<UniversalKey> Keys
		{
			get { return TableLink.KeysForIndex(IndexBlank); }
		}
	}

	public class MultiIndexLink<T>
		where T : class
	{
		public readonly TableLink<T> TableLink;
		public readonly MultiIndexBlank<T> IndexBlank;

		public MultiIndexLink(TableLink<T> tableLink, MultiIndexBlank<T> indexBlank)
		{
			this.TableLink = tableLink;
			this.IndexBlank = indexBlank;
		}

		public T[] Rows(params object[] keyParts)
		{
			return TableLink.FindRows(IndexBlank, keyParts);
		}

		public bool Exist(params object[] keyParts)
		{
			return Rows(keyParts).Length != 0;
		}

		public ICollection<UniversalKey> Keys
		{
			get { return TableLink.KeysForIndex(IndexBlank); }
		}
	}

	public class OrderedIndexLink<T>
		where T : class
	{
		public readonly TableLink<T> TableLink;
		public readonly OrderedIndexBlank<T> IndexBlank;

		public OrderedIndexLink(TableLink<T> tableLink, OrderedIndexBlank<T> indexBlank)
		{
			this.TableLink = tableLink;
			this.IndexBlank = indexBlank;
		}

		public TField? Get<TField>(IPropertyBlank<T, TField> property, int orderIndex, params object[] basicKeyParts)
		{
			T? row = Row(property.Kind, orderIndex, basicKeyParts);
			if (row == null)
				return default;
			return property.Field.GetValue(row);
		}

		public bool Set<TField>(IPropertyBlank<T, TField> property, int orderIndex, TField value, params object[] basicKeyParts)
		{
			T? row = Row(property.Kind, orderIndex, basicKeyParts);
			if (row == null)
				return false;
			property.Field.SetValue(row, value);
			return true;
		}

		public T? Row(int propertyKind, int orderIndex, params object[] basicKeyParts)
		{
			T[] rows = Rows(propertyKind, basicKeyParts);
			if (rows.Length == 0)
				return null;

			if (rows.Length == 1)
			{
				if (IndexBlank.OrderField.GetValue(rows[0]) == orderIndex)
					return rows[0];
				return null;
			}
			int position = _.BinarySearch(rows, orderIndex,
				delegate (T row) { return IndexBlank.OrderField.GetValue(row); }, Comparer<int>.Default.Compare);
			if (position < 0)
				return null;

			return rows[position];
		}

		public T[] Rows(int propertyKind, params object[] basicKeyParts)
		{
			return TableLink.FindRows(IndexBlank, ArrayHlp.Merge(basicKeyParts, new object[] { propertyKind }));
		}

		public void InsertInArray(T insertItem, int insertIndex)
		{
			T[] rows = TableLink.FindRows(IndexBlank, IndexBlank.CreateKey(insertItem).KeyParts);
			InsertInArray(rows, insertItem, insertIndex);
		}

		void InsertInArray(T[] rows, T insertArrayItem, int insertIndex)
		{
			if (insertIndex > rows.Length)
				insertIndex = rows.Length;

			IndexBlank.OrderField.SetValue(insertArrayItem, insertIndex);

			bool isLeftConflict = false;
			if (insertIndex > 0)
				isLeftConflict = (IndexBlank.OrderField.GetValue(rows[insertIndex - 1]) != insertIndex - 1);
			bool isRightConflict = insertIndex < rows.Length;

			if (isLeftConflict)
			{
				for (int i = 0; i < insertIndex; ++i)
					TableLink.RemoveIndexForRow(rows[i]);
			}
			if (isRightConflict)
			{
				for (int i = insertIndex; i < rows.Length; ++i)
					TableLink.RemoveIndexForRow(rows[i]);
			}

			if (isLeftConflict)
			{
				for (int i = 0; i < insertIndex; ++i)
				{
					IndexBlank.OrderField.SetValue(rows[i], i);
					TableLink.CreateIndexForRow(rows[i]);
				}
			}

			if (isRightConflict)
			{
				for (int i = insertIndex; i < rows.Length; ++i)
				{
					IndexBlank.OrderField.SetValue(rows[i], i + 1);
					TableLink.CreateIndexForRow(rows[i]);
				}
			}

			TableLink.AddRow(insertArrayItem);
		}

		public void AddInArray(T addArrayItem)
		{
			T[] rows = TableLink.FindRows(IndexBlank, IndexBlank.CreateKey(addArrayItem).KeyParts);
			InsertInArray(rows, addArrayItem, rows.Length);
		}

		public bool RemoveFromArray(IPropertyBlank<T> propertyKind, int removeArrayIndex,
			params object[] basicKeyParts)
		{
			T[] rows = Rows(propertyKind.Kind, basicKeyParts);

			if (removeArrayIndex >= rows.Length)
				return false;

			TableLink.RemoveRow(rows[removeArrayIndex]);

			for (int i = removeArrayIndex + 1; i < rows.Length; ++i)
				TableLink.RemoveIndexForRow(rows[i]);

			for (int i = removeArrayIndex + 1; i < rows.Length; ++i)
			{
				IndexBlank.OrderField.SetValue(rows[i], i - 1);
				TableLink.CreateIndexForRow(rows[i]);
			}
			return true;
		}

		//public T GetOrCreate(Func<T> rowCreator, int propertyKind, int orderIndex, params object[] basicKeyParts)
		//{
		//	T? findRow = Row(propertyKind, orderIndex, basicKeyParts);

		//	if (findRow != null)
		//		return findRow;

		//	T createRow = rowCreator();
		//	TableLink.AddRow(createRow);
		//	return createRow;
		//}
	}
}
