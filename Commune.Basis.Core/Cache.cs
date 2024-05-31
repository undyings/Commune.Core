using System;
using System.Collections.Generic;
using System.Text;

namespace Commune.Basis
{

  public interface ICache<TResult>
  {
    TResult Result { get; }
    long ChangeTick { get; }  
  }

  public class RawCache<TResult> : ICache<TResult>
  {
    public RawCache(Func<object[], TResult> resulter, params Func<object>[] sourcers)
    {
      this.resulter = resulter;
      this.sourcers = sourcers;
      this.args_Old = new object[sourcers.Length];
    }
		Func<object[], TResult> resulter;
    Func<object>[] sourcers;

    bool isInited = false;
    object[] args_Old;
    TResult result_cache;
    public long ChangeTick 
    { 
      get 
      {
        CheckAndUpdateData();
        return _ChangeTick; 
      } 
    }
    long _ChangeTick = 1;
    public TResult Result
    {
      get
      {
        CheckAndUpdateData();
        return result_cache;
      }
    }

    public bool IsInited
    {
      get { return isInited; }
    }

    public TResult ResultWithoutCheckAndUpdate
    {
      get { return result_cache; }
    }

    public long ChangeTickWithoutCheckAndUpdate
    {
      get { return _ChangeTick; }
    }

    private void CheckAndUpdateData()
    {
      object[] args = new object[sourcers.Length];
      for (int i = 0; i < sourcers.Length; ++i)
      {
        args[i] = sourcers[i]();
      }
      bool isChanged = false;
      for (int i = 0; i < sourcers.Length; ++i)
      {
        if (!object.Equals(args[i], args_Old[i]))
        {
          isChanged = true;
          break;
        }
      }

      if (!isInited || isChanged)
      {
        TResult result = resulter(args);

        result_cache = result;
        args_Old = args;
        isInited = true;
        _ChangeTick++;
      }
    }
  }

  public class Cache<TArg1, TResult> : RawCache<TResult> 
    where TArg1 : notnull
  {
    public Cache(Func<TArg1, TResult> resulter, Func<TArg1> sourcer1)
      :
      base(delegate(object[] args) { return resulter((TArg1)args[0]); },
      delegate { return sourcer1(); })
    {
    }
  }
  public class Cache<TArg1, TArg2, TResult> : RawCache<TResult> 
    where TArg1 : notnull 
    where TArg2 : notnull
	{
    public Cache(Func<TArg1, TArg2, TResult> resulter, Func<TArg1> sourcer1, Func<TArg2> sourcer2)
      :
      base(delegate(object[] args) { return resulter((TArg1)args[0], (TArg2)args[1]); },
      delegate { return sourcer1(); },
      delegate { return sourcer2(); })
    {
    }
  }

  public class Cache<TArg1, TArg2, TArg3, TResult> : RawCache<TResult>
		where TArg1 : notnull
		where TArg2 : notnull
    where TArg3 : notnull
	{
		public Cache(Func<TArg1, TArg2, TArg3, TResult> resulter,
      Func<TArg1> sourcer1,
      Func<TArg2> sourcer2,
      Func<TArg3> sourcer3
      )
      :
      base(delegate(object[] args) { return resulter((TArg1)args[0], (TArg2)args[1], (TArg3)args[2]); },
      delegate { return sourcer1(); },
      delegate { return sourcer2(); },
      delegate { return sourcer3(); })
    {
    }
  }
  public class Cache<TArg1, TArg2, TArg3, TArg4, TResult> : RawCache<TResult>
		where TArg1 : notnull
		where TArg2 : notnull
		where TArg3 : notnull
    where TArg4 : notnull
	{
    public Cache(Func<TArg1, TArg2, TArg3, TArg4, TResult> resulter,
      Func<TArg1> sourcer1,
      Func<TArg2> sourcer2,
      Func<TArg3> sourcer3,
      Func<TArg4> sourcer4
      )
      :
      base(delegate(object[] args) { return resulter((TArg1)args[0], (TArg2)args[1], (TArg3)args[2], (TArg4)args[3]); },
      delegate { return sourcer1(); },
      delegate { return sourcer2(); },
      delegate { return sourcer3(); },
      delegate { return sourcer4(); })
    {
    }
  }
  public class Cache<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> : RawCache<TResult>
		where TArg1 : notnull
		where TArg2 : notnull
		where TArg3 : notnull
		where TArg4 : notnull
    where TArg5 : notnull
	{
    public Cache(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> resulter,
      Func<TArg1> sourcer1,
			Func<TArg2> sourcer2,
			Func<TArg3> sourcer3,
			Func<TArg4> sourcer4,
			Func<TArg5> sourcer5
      )
      :
      base(delegate(object[] args) { return resulter((TArg1)args[0], (TArg2)args[1], (TArg3)args[2], (TArg4)args[3], (TArg5)args[4]); },
      delegate { return sourcer1(); },
      delegate { return sourcer2(); },
      delegate { return sourcer3(); },
      delegate { return sourcer4(); },
      delegate { return sourcer5(); })
    {
    }
  }
  public class Cache<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> : RawCache<TResult>
		where TArg1 : notnull
		where TArg2 : notnull
		where TArg3 : notnull
		where TArg4 : notnull
		where TArg5 : notnull
    where TArg6 : notnull
	{
    public Cache(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> resulter,
			Func<TArg1> sourcer1,
			Func<TArg2> sourcer2,
			Func<TArg3> sourcer3,
			Func<TArg4> sourcer4,
			Func<TArg5> sourcer5,
			Func<TArg6> sourcer6
      )
      :
      base(delegate(object[] args) { return resulter((TArg1)args[0], (TArg2)args[1], (TArg3)args[2], (TArg4)args[3], (TArg5)args[4], (TArg6)args[5]); },
      delegate { return sourcer1(); },
      delegate { return sourcer2(); },
      delegate { return sourcer3(); },
      delegate { return sourcer4(); },
      delegate { return sourcer5(); },
      delegate { return sourcer6(); })
    {
    }
  }
  public class Cache<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> : RawCache<TResult>
		where TArg1 : notnull
		where TArg2 : notnull
		where TArg3 : notnull
		where TArg4 : notnull
		where TArg5 : notnull
		where TArg6 : notnull
    where TArg7 : notnull
	{
    public Cache(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> resulter,
			Func<TArg1> sourcer1,
			Func<TArg2> sourcer2,
			Func<TArg3> sourcer3,
			Func<TArg4> sourcer4,
			Func<TArg5> sourcer5,
			Func<TArg6> sourcer6,
			Func<TArg6> sourcer7
      )
      :
      base(delegate(object[] args) { return resulter((TArg1)args[0], (TArg2)args[1], (TArg3)args[2], (TArg4)args[3], (TArg5)args[4], (TArg6)args[5], (TArg7)args[6]); },
      delegate { return sourcer1(); },
      delegate { return sourcer2(); },
      delegate { return sourcer3(); },
      delegate { return sourcer4(); },
      delegate { return sourcer5(); },
      delegate { return sourcer6(); },
      delegate { return sourcer7(); })
    {
    }
  }
}
