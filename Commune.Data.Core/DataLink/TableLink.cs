using Commune.Basis;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class TableLink<T>
		where T : class
	{
		readonly IndexBlank<T> primaryIndex;
		readonly IndexBlank<T>[] indices;

		readonly Dictionary<string, Dictionary<UniversalKey, T>> singleIndicesByName = new();
		readonly Dictionary<string, Dictionary<UniversalKey, List<T>>> multiIndicesByName = new();

		public TableLink(T[] rows, IndexBlank<T> primaryIndex, params IndexBlank<T>[] nonPrimaryIndices)
		{
			this.primaryIndex = primaryIndex;
			this.indices = ArrayHlp.Merge(primaryIndex, nonPrimaryIndices);

			foreach (IndexBlank<T> index in indices)
			{
				if (index.IsMultiIndex)
					multiIndicesByName[index.IndexName] = new Dictionary<UniversalKey, List<T>>();
				else
					singleIndicesByName[index.IndexName] = new Dictionary<UniversalKey, T>();
			}

			foreach (T row in rows)
			{
				CreateIndexForRow(row);
			}
		}

		long rowListChangeTick = 0;
		public long RowListChangeTick
		{
			get { return rowListChangeTick; }
		}
		long dataChangeTick = 0;
		public long DataChangeTick
		{
			get { return rowListChangeTick + dataChangeTick; }
		}

		public void IncrementDataChangeTick()
		{
			dataChangeTick++;
		}

		public void AddRow(T newRow)
		{
			CreateIndexForRow(newRow);
			rowListChangeTick++;

		}

		public void RemoveRow(T row)
		{
			RemoveIndexForRow(row);
			rowListChangeTick++;
		}


		public T? FindRow(SingleIndexBlank<T> index, params object[] keyParts)
		{
			T? row;
			if (singleIndicesByName[index.IndexName].TryGetValue(new UniversalKey(keyParts), out row))
				return row;
			return default;
		}

		public T? FindRow(MultiIndexBlank<T> index, params object[] keyParts)
		{
			T[] rows = FindRows(index, keyParts);
			return rows.FirstOrDefault();
		}

		public T[] FindRows(MultiIndexBlank<T> index, params object[] keyParts)
		{
			List<T>? rows;
			if (multiIndicesByName[index.IndexName].TryGetValue(new UniversalKey(keyParts), out rows))
				return rows.ToArray();
			return Array.Empty<T>();
		}

		public T[] FindRows(IndexBlank<T> index, params object[] keyParts)
		{
			if (index.IsMultiIndex)
				return FindRows((MultiIndexBlank<T>)index, keyParts);

			T? row = FindRow((SingleIndexBlank<T>)index, keyParts);
			return row != null ? new T[] { row } : Array.Empty<T>();
		}

		public T[] AllRows
		{
			get
			{
				foreach (Dictionary<UniversalKey, T> index in singleIndicesByName.Values)
					return index.Values.ToArray();

				foreach (Dictionary<UniversalKey, List<T>> index in multiIndicesByName.Values)
				{
					List<T> allRows = new List<T>();
					foreach (List<T> rows in index.Values)
						allRows.AddRange(rows);
					return allRows.ToArray();
				}

				return Array.Empty<T>();
			}
		}

		public ICollection<UniversalKey> KeysForIndex(IndexBlank<T> index)
		{
			if (index.IsMultiIndex)
				return multiIndicesByName[index.IndexName].Keys;
			else
				return singleIndicesByName[index.IndexName].Keys;
		}

		public void CreateIndexForRow(T row)
		{
			foreach (IndexBlank<T> index in this.indices)
			{
				UniversalKey key = index.CreateKey(row);
				if (index.IsMultiIndex)
				{
					Dictionary<UniversalKey, List<T>> rowsForKey = multiIndicesByName[index.IndexName];
					List<T>? rows;
					if (!rowsForKey.TryGetValue(key, out rows))
					{
						rows = new List<T>();
						rowsForKey[key] = rows;
					}
					if (index is OrderedIndexBlank<T>)
					{
						_.InsertInSortedList(rows, row, ((OrderedIndexBlank<T>)index).OrderField.GetValue);
					}
					else
					{
						rows.Add(row);
					}
				}
				else
				{
					Dictionary<UniversalKey, T> singleIndex = singleIndicesByName[index.IndexName];
					if (singleIndex.ContainsKey(key))
						Log.Information("Дубль значения '{0}' по индексу '{1}'", key, index.IndexName);
					singleIndex[key] = row;
				}
			}
		}

		public void RemoveIndexForRow(T row)
		{
			foreach (IndexBlank<T> index in indices)
			{
				UniversalKey key = index.CreateKey(row);
				if (index.IsMultiIndex)
					multiIndicesByName[index.IndexName][key].Remove(row);
				else
					singleIndicesByName[index.IndexName].Remove(key);
			}
		}

		public void SyncChanges(DbSet<T> dbSet, T[] dbRows, Action<T, T> rowSyncChanges)
		{
			T[] actualRows = AllRows;
			CollectionHlp.Synchronize<T, T, UniversalKey>(dbRows, primaryIndex.CreateKey, actualRows, primaryIndex.CreateKey,
				delegate (T onlyLeft)
				{
					dbSet.Remove(onlyLeft);
				},
				delegate (T onlyRight)
				{
					dbSet.Add(onlyRight);
				},
				delegate (T old, T actual)
				{
					rowSyncChanges(old, actual);
				}
			);
		}

	}
}
