using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Commune.Basis;
using Serilog;

namespace Commune.Task
{
  public class TaskPull
  {
    readonly object lockObj = new();

    readonly Dictionary<string, BackgroundPullThread> threadByLabel = new Dictionary<string, BackgroundPullThread>();
    readonly TimeSpan maxWorkingStepTime;

    public TaskPullStatus Status
    {
      get
      {
        BackgroundPullThread[] threads;
        lock (lockObj)
        {
          threads = threadByLabel.Values.ToArray();
        }
        List<LabeledThreadStatus> threadsStatus = new List<LabeledThreadStatus>();
        List<string> hoverThreads = new List<string>();
        foreach (BackgroundPullThread thread in threads)
        {
          threadsStatus.Add(thread.Status);
          if (thread.Status.WorkingCurrentStepTime > maxWorkingStepTime)
            hoverThreads.Add(thread.Label);
        }
        return new TaskPullStatus(threads.Length, maxWorkingStepTime, hoverThreads.ToArray(), threadsStatus.ToArray());
      }
    }

    public Task StartTask(ThreadLabel threadLabel, IEnumerable<Step> taskSteps)
    {
      return StartTask(threadLabel, new Task(taskSteps));
    }

    public Task StartTask(string threadLabel, Task task)
    {
      return StartTask(new ThreadLabel(threadLabel, ThreadPriority.Normal), task);
    }

    public Task StartTask(ThreadLabel threadLabel, Task task)
    {
      BackgroundPullThread? thread;
      lock (lockObj)
      {
        if (!threadByLabel.TryGetValue(threadLabel.Label, out thread))
        {
          thread = new BackgroundPullThread(threadLabel);
          threadByLabel[threadLabel.Label] = thread;
          Log.Information("��������� ����� ��� ��� ����� '{0}' � ����������� '{1}'", threadLabel.Label, threadLabel.Priority);
        }
      }
      ((IForceTask)task).Initialize(thread);
      thread.AddTask(task);
      return task;
    }

    volatile bool isFinishing = false;
    public bool IsFinishing
    {
      get { return isFinishing; }
    }

    public void Finish()
    {
      isFinishing = true;
      if (isFinishing)
      {
        foreach (BackgroundPullThread thread in threadByLabel.Values)
          thread.Finish();
      }
    }

    public bool HoverThreadExist
    {
      get
      {
        string[] hoverThreads = Status.HoverThreads;
        if (hoverThreads != null && hoverThreads.Length != 0)
          return true;
        return false;
      }
    }

    public string[] HoverThreads
    {
      get
      {
        return Status.HoverThreads;
      }
    }

    public TaskPull(ICollection<ThreadLabel> labels, TimeSpan maxWorkingStepTime)
    {
      this.maxWorkingStepTime = maxWorkingStepTime;

      foreach (ThreadLabel label in labels)
      {
        try
        {
          threadByLabel.Add(label.Label, new BackgroundPullThread(label));
        }
        catch (Exception ex)
        {
          Log.Error(ex, "");
        }
      }
    }
  }
}
