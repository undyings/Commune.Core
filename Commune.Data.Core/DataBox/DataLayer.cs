using Commune.Basis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class DataLayer
	{
		readonly object lockObj = new();

		protected readonly Func<BoxDbContext> dbContextCreator;
		readonly Dictionary<string, int> maxIdByTableName;

		public DataLayer(Func<BoxDbContext> dbContextCreator, params Tuple<string, int>[] maxIdForTables)
		{
			this.dbContextCreator = dbContextCreator;
			this.maxIdByTableName = maxIdForTables.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
		}

		public BoxDbContext Create()
		{
			return dbContextCreator();
		}

		public int GeneratePrimaryKey(string tableName)
		{
			lock (lockObj)
			{
				if (!maxIdByTableName.TryGetValue(tableName, out int maxId))
					throw new Exception($"Для таблицы '{tableName}' не найден maxPrimaryKey");

				maxIdByTableName[tableName] = maxId + 1;
				return maxId + 1;
			}
		}
	}

	//public class DataLayer
	//{
	//	readonly object lockObj = new();

	//	protected readonly Func<BoxDbContext> dbContextCreator;
	//	readonly int reservationStep;

	//	public DataLayer(Func<BoxDbContext> dbContextCreator, int reservationStep)
	//	{
	//		this.dbContextCreator = dbContextCreator;
	//		this.reservationStep = reservationStep;
	//	}

	//	public BoxDbContext Create()
	//	{
	//		return dbContextCreator();
	//	}

	//	PrimaryKeyRow InitPrimaryKey(string tableName, BoxDbContext dbContext)
	//	{
	//		PrimaryKeyRow? keyRow = dbContext.PrimaryKeys.Where(key => key.TableName == tableName).FirstOrDefault();
	//		if (keyRow == null)
	//		{
	//			//hack резервируем диапазон для захардкоденных свойств
	//			int startIndex = tableName == BoxTableNames.ObjectTable ? 100000 : 0;

	//			currentPrimaryKey = startIndex;
	//			maxPrimaryKey = startIndex;
	//			keyRow = new PrimaryKeyRow { TableName = tableName, MaxPrimaryKey = maxPrimaryKey };
	//			dbContext.PrimaryKeys.Add(keyRow);
	//		}
	//		else if (currentPrimaryKey == -1)
	//		{
	//			currentPrimaryKey = keyRow.MaxPrimaryKey;
	//		}

	//		return keyRow;
	//	}

	//	int maxPrimaryKey = -1;
	//	int currentPrimaryKey = -1;

	//	public int GeneratePrimaryKey(string tableName)
	//	{
	//		lock (lockObj)
	//		{
	//			if (currentPrimaryKey >= maxPrimaryKey)
	//			{
	//				using (BoxDbContext dbContext = this.dbContextCreator())
	//				{
	//					//PrimaryKeyRow? keyRow = dbContext.PrimaryKeys.Where(key => key.TableName == tableName).FirstOrDefault();
	//					//if (keyRow == null)
	//					//{
	//					//	//hack резервируем диапазон для захардкоденных свойств
	//					//	int startIndex = tableName == BoxTableNames.ObjectTable ? 100000 : 0;

	//					//	currentPrimaryKey = startIndex;
	//					//	maxPrimaryKey = startIndex;
	//					//	keyRow = new PrimaryKeyRow { TableName = tableName, MaxPrimaryKey = maxPrimaryKey };
	//					//	dbContext.PrimaryKeys.Add(keyRow);
	//					//}
	//					//else if (currentPrimaryKey == -1)
	//					//{
	//					//	currentPrimaryKey = keyRow.MaxPrimaryKey;
	//					//}

	//					PrimaryKeyRow keyRow = InitPrimaryKey(tableName, dbContext);

	//					keyRow.MaxPrimaryKey += reservationStep;

	//					dbContext.SaveChanges();

	//					maxPrimaryKey = keyRow.MaxPrimaryKey;
	//				}
	//			}

	//			currentPrimaryKey++;
	//			return currentPrimaryKey;
	//		}
	//	}

	//	public void ReservationDiapason(string tableName, int diapason)
	//	{
	//		lock (lockObj)
	//		{
	//			using (BoxDbContext dbContext = this.dbContextCreator())
	//			{
	//				PrimaryKeyRow keyRow = InitPrimaryKey(tableName, dbContext);

	//				keyRow.MaxPrimaryKey = currentPrimaryKey + diapason;

	//				dbContext.SaveChanges();

	//				maxPrimaryKey = keyRow.MaxPrimaryKey;
	//			}
	//		}
	//	}
	//}
}
