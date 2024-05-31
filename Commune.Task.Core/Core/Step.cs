using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;

namespace Commune.Task
{
  public class Step { }
  public class WaitStep : Step
  {
    public WaitStep(Func<bool> isEnd) :
      this(TimeSpan.FromMilliseconds(100), isEnd)
    {
    }

    public WaitStep(TimeSpan maxExecuteRate) :
      this(maxExecuteRate, null)
    {
    }

    public WaitStep(TimeSpan maxExecuteRate, Func<bool>? isEnd)
    {
      this.MaxExecuteRate = maxExecuteRate;
      this.isEnd = isEnd;
    }
    public readonly TimeSpan MaxExecuteRate;
    readonly Func<bool>? isEnd = null;

    public bool IsEnd
    {
      get
      {
        if (isEnd == null)
          return true;
        return isEnd();
      }
    }
  }
  public class FinishStep : Step
  {
    public FinishStep(object result)
    {
      this.Result = result;
    }
    public readonly object Result;
  }
}