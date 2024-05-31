using Microsoft.EntityFrameworkCore;
using Serilog;
using Commune.Basis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class SqliteHlp
	{
		public static bool TableExist(DbContext context, string tableName)
		{
			return context.Database.SqlQuery<int>($"Select count(*) From sqlite_master Where name = {tableName}").FirstOrDefault() != 0;
		}

		public static int GenerateNewPrimaryKey(DbContext context, string tableName)
		{
			return context.Database.SqlQuery<int>($@"update light_primary_key set max_primary_key = max_primary_key + 1 where table_name = '{tableName}';
        select max_primary_key from light_primary_key where table_name = '{tableName}';"
			).First();
		}

		public static void CheckAndCreateDataBoxTables(DbContext context)
		{
			if (!TableExist(context, BoxTableNames.ObjectTable))
			{
				CreateTableForObjects(context);
				Log.Information("Создана таблица объектов");
			}

			if (!TableExist(context, BoxTableNames.PropertyTable))
			{
				CreateTableForProperties(context);
				Log.Information("Создана таблица для свойств объектов");
			}

			if (!TableExist(context, BoxTableNames.LinkTable))
			{
				CreateTableForLinks(context);
				Log.Information("Создана таблица ссылок на объекты");
			}

			if (!TableExist(context, BoxTableNames.PrimaryKeyTable))
			{
				CreateTableForPrimaryKey(context);
				Log.Information("Создана таблица табличных первичных ключей");
			}

			Tuple<string, string>[] allTables = new Tuple<string, string>[] {
				_.Tuple(BoxTableNames.ObjectTable, "obj_id"), _.Tuple(BoxTableNames.PropertyTable, "prop_id"),
				_.Tuple(BoxTableNames.LinkTable, "link_id")
			};

			foreach (Tuple<string, string> table in allTables)
			{
				if (context.Database.SqlQuery<int>(
					$"Select count(*) From {BoxTableNames.PrimaryKeyTable} Where table_name = '{table.Item1}'").FirstOrDefault() == 0)
				{
					int maxId = context.Database.SqlQuery<int>($"Select max({table.Item2}) From {table.Item1}").FirstOrDefault();

					//hack резервируем диапазон для захардкоденных свойств
					if (table.Item1 == BoxTableNames.ObjectTable && maxId < 100000)
						maxId = 100000;

          context.Database.ExecuteSql($"Insert Into {BoxTableNames.PrimaryKeyTable} values ('{table.Item1}', {maxId})");
					Log.Information("Добавлен max_primary_key = '{0}' для таблицы {1}", maxId, table.Item1);
				}
			}
		}

		public static void CreateTableForObjects(DbContext context)
		{
			context.Database.ExecuteSql(
				@$"CREATE TABLE light_object (
            obj_id    integer PRIMARY KEY NOT NULL,
            type_id   integer NOT NULL,
            json_ids  text NOT NULL,
            act_from  datetime,
            act_to  datetime
          );

          CREATE INDEX light_object_by_type_act_from
            ON light_object
            (type_id, act_from);

          CREATE INDEX light_object_by_type_act_to
            ON light_object
            (type_id, act_to);

          CREATE INDEX light_object_by_type_xml_attrs
            ON light_object
            (type_id, xml_ids);"
			);
		}

		public static void CreateTableForProperties(DbContext context)
		{
			context.Database.ExecuteSql(
				@$"CREATE TABLE light_property (
            prop_id     integer PRIMARY KEY NOT NULL,
            obj_id      integer NOT NULL,
            type_id     integer NOT NULL,
            prop_index  integer NOT NULL DEFAULT 0,
            prop_value  text
          );

          CREATE UNIQUE INDEX light_property_by_obj_type_index
            ON light_property
            (obj_id, type_id, prop_index);

          CREATE INDEX light_property_by_type_id
            ON light_property
            (type_id);"
			);
		}

		public static void CreateTableForLinks(DbContext context)
		{
			context.Database.ExecuteSql(
				@$"CREATE TABLE light_link (
            link_id     integer PRIMARY KEY NOT NULL,
            parent_id   integer NOT NULL,
            type_id     integer NOT NULL,
            link_index  integer NOT NULL,
            child_id    integer NOT NULL
          );

          CREATE INDEX light_link_by_child_type_id
            ON light_link
            (child_id, type_id);

          CREATE UNIQUE INDEX light_link_by_parent_type_id
            ON light_link
            (parent_id, type_id, link_index);"
			);
		}

		public static void CreateTableForPrimaryKey(DbContext context)
		{
			context.Database.ExecuteSql(
				@$"CREATE TABLE light_primary_key (
            table_name  text PRIMARY KEY NOT NULL,
            max_primary_key integer
        );"
			);
		}

		public static void DeleteParentObject(BoxDbContext context, int objectId)
		{
			DeleteRecursiveChildObject(context, objectId);

			context.Database.ExecuteSql($"Delete From light_link Where child_id = {objectId}");
		}

		static void DeleteRecursiveChildObject(BoxDbContext context, int objectId)
		{
			LinkRow[] links = context.Links.Where(link => link.ParentId == objectId).ToArray();

			foreach (LinkRow link in links)
			{
				DeleteRecursiveChildObject(context, link.ChildId);
			}

			if (links.Length > 0)
			{
				context.Database.ExecuteSql($"Delete From light_link Where parent_id = {objectId}");
			}

			context.Database.ExecuteSql($"Delete From light_property Where obj_id = {objectId}");
			context.Database.ExecuteSql($"Delete From light_object Where obj_id = {objectId}");
		}

	}
}
