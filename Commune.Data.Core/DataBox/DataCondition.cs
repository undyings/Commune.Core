using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Commune.Basis;
using Microsoft.EntityFrameworkCore;

namespace Commune.Data
{
  public static class DataCondition
  {
    //public static string ForEnum(string columnName, IEnumerable<UniversalKey> keys)
    //{
    //  List<string> keysAsStr = new List<string>();
    //  foreach (UniversalKey key in keys)
    //    keysAsStr.Add(key.KeyParts[0].ToString());
    //  if (keysAsStr.Count == 0)
    //    return "1=0";
    //  return string.Format("{0} in ({1})", columnName, string.Join(",", keysAsStr.ToArray()));
    //}

    //public static string ForEnum(string conditionColumn, params int[] conditionIds)
    //{
    //  if (conditionIds.Length == 0)
    //    return "1=0";

    //  return string.Format("{0} in ({1})", conditionColumn, StringHlp.Join(",", "{0}", conditionIds));
    //}

    public static IQueryable<ObjectRow> ForTypeObjects(this DbSet<ObjectRow> objects, int typeId, params int[] objectIds)
    {
      return objects.Where(obj => objectIds.Contains(obj.ObjectId) && obj.TypeId == typeId);
		}

    public static IQueryable<ObjectRow> ForObjects(this DbSet<ObjectRow> objects, params int[] objectIds)
    {
      return objects.Where(obj => objectIds.Contains(obj.ObjectId));
		}

    //public static IQueryable<ObjectRow> ForEnumString(this DbSet<ObjectRow> objects, string conditionColumn, params string[] conditionIds)
    //{
    //  return string.Format("{0} in ({1})", conditionColumn, StringHlp.Join(",", "'{0}'", conditionIds));
    //}

    public static IQueryable<ObjectRow> ForChilds(this DbSet<ObjectRow> objects, ParentBox parentBox, params LinkBlank[] linkKinds)
    {
      List<int> childIds = new();

      Dictionary<int, bool> linkKindByTypeId = new();
      foreach (LinkBlank linkKind in linkKinds)
        linkKindByTypeId[linkKind.Kind] = true;

      foreach (LinkRow childRow in parentBox.ChildByLinkId.TableLink.AllRows)
      {
        if (linkKindByTypeId.ContainsKey(childRow.TypeId))
          childIds.Add(childRow.ChildId);
      }
      return objects.Where(obj => childIds.Contains(obj.ObjectId));
    }

    public static IQueryable<ObjectRow> ForTypes(this DbSet<ObjectRow> objects, params int[] typeIds)
    {
      return objects.Where(obj => typeIds.Contains(obj.TypeId));
    }

    public static IQueryable<ObjectRow> ForType(this DbSet<ObjectRow> objects, int typeId)
    {
      return objects.Where(obj => obj.TypeId == typeId);
		}
  }
}
