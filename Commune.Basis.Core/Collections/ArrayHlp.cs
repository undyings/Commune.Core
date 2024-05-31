#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

#endregion

namespace Commune.Basis
{
  /// <summary>
  /// Алгоритмы/Методы для работы с массивами
  /// </summary>
  public class ArrayHlp
  {
    public static void Copy<T>(T[] sourceArray, T[] destinationArray, int destinationIndex)
    {
      Array.Copy(sourceArray, 0, destinationArray, destinationIndex, sourceArray.Length);
    }

    public static T[] Merge<T>(T first, T[] array)
    {
      return Merge(new T[] { first }, array);
    }

    public static T[] Merge<T>(params T[][] arrays)
    {
      int length = 0;
      foreach (T[] array in arrays)
        length += array.Length;

      T[] result = new T[length];
      int index = 0;
      foreach (T[] array in arrays)
      {
        Array.Copy(array, 0, result, index, array.Length);
        index += array.Length;
      }
      return result;
    }

    public static T[] ToArray<T>(T item) where T : class
    {
      if (item == null)
        return new T[0];
      return new T[] { item };
    }
    public static T[] ToArray<T>(T? item) where T : struct
    {
      if (item == null)
        return new T[0];
      return new T[] { item.Value };
    }

    //public static void MoveSelectedElements<T>(List<T> c, int startSelectionIndex, int selectionCount, int offset)
    //{
    //  int insertIndex = MathHlp.Bound(startSelectionIndex + offset, 0, c.Count - selectionCount);
    //  List<T> selection = new List<T>(c.GetRange(startSelectionIndex, selectionCount));
    //  c.RemoveRange(startSelectionIndex, selectionCount);
    //  c.InsertRange(insertIndex, selection);
    //}

    public static T[] GetRange<T>(T[] c, int index, int count)
    {
      int resultCount = Math.Min(c.Length - index, count);
      if (resultCount <= 0)
        return Array.Empty<T>();

      T[] arr = new T[resultCount];
      for (int i = 0; i < resultCount; ++i)
      {
        arr[i] = c[i + index];
      }
      return arr;
    }

    public static T[] GetInterval<T>(T[] source, int startIndex, int endIndex)
    {
      return GetRange(source, startIndex, endIndex - startIndex);
      //T[] objArray = new T[endIndex - startIndex];
      //for (int index = 0; index < objArray.Length; ++index)
      //  objArray[index] = source[startIndex + index];
      //return objArray;
    }

    public static T[] Split<T>(T[] source, int splitIndex, out T[] rest)
    {
      rest = ArrayHlp.GetInterval<T>(source, splitIndex, source.Length);
      return ArrayHlp.GetInterval<T>(source, 0, splitIndex);
    }

    public static bool Equals<T>(T[] array1, T[] array2)
    {
      if (array1.Length != array2.Length)
        return false;

      for (int i = 0; i < array1.Length; ++i)
      {
        if (!object.Equals(array1[i], array2[i]))
          return false;
      }
      return true;
    }

    //public static bool CompareByEachElement<T>(IEnumerable<T> c1, IEnumerable<T> c2) //where T: class
    //{
    //  if (c1 == null && c2 == null)
    //    return true;
    //  if (c1 == null || c2 == null)
    //    return false;
    //  //int count1 = c1.Count;
    //  //if (count1 != c2.Count)
    //  //return false;
    //  IEnumerator<T> iE1 = c1.GetEnumerator();
    //  IEnumerator<T> iE2 = c2.GetEnumerator();

    //  for (; ; )
    //  {
    //    bool isNext1 = iE1.MoveNext();
    //    bool isNext2 = iE2.MoveNext();
    //    if (isNext1 != isNext2)
    //      return false;
    //    if (!isNext1 || !isNext2) // if((!IsNext1)
    //      break;
    //    if (!ObjectHlp.IsEquals<T>(iE1.Current, iE2.Current))
    //      return false;
    //  }
    //  return true;
    //}

    //public static bool CompareByEachElement<T>(IList<T> c1, IList<T> c2, int count)
    //{
    //  return CompareByEachElement(c1, 0, c2, 0, count);
    //}

    //public static bool CompareByEachElement<T>(IList<T> c1, int index1, IList<T> c2, int index2, int count)
    //{
    //  if (c1.Count < count + index1)
    //    throw new ArgumentException("длина c1 меньше count", "c1");
    //  if (c2.Count < count + index2)
    //    throw new ArgumentException("длина c2 меньше count", "c2");

    //  for (int i = 0; i < count; ++i)
    //  {
    //    if (!ObjectHlp.IsEquals(c1[i + index1], c2[i + index2]))
    //      return false;
    //  }
    //  return true;
    //}

    public static TDest[] Convert<TSource, TDest>(ICollection<TSource> sourceCollection,
      Func<TSource, TDest> converter)
    {
      TDest[] result = new TDest[sourceCollection.Count];
      int index = -1;
      foreach (TSource item in sourceCollection)
      {
        index++;
        result[index] = converter(item);
      }
      return result;
    }

    public static void RemoveBetween<T>(List<T> arr, int startIndex, int finishIndex)
    {
      if (finishIndex < startIndex)
        return;
      arr.RemoveRange(startIndex, finishIndex - startIndex + 1);
    }

    public static void Sort<TKey, TItem>(TItem[] array, Func<TItem, TKey> getter)
      where TKey : IComparable
    {
      Array.Sort(array, delegate(TItem s1, TItem s2) { return getter(s1).CompareTo(getter(s2)); });
    }

    public static void Sort<TKey, TItem>(TItem[] array, Func<TItem, TKey> getter,
      Comparison<TKey> comparer)
    {
      Array.Sort(array, delegate(TItem s1, TItem s2) { return comparer(getter(s1), getter(s2)); });
    }
  }
}
