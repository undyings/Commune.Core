using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Commune.Task
{
  public class LabeledThreadStatus
  {
    public string Label = "";
    public int TaskCount = 0;
    [XmlIgnore()]
    public readonly TimeSpan? WorkingCurrentStepTime;
    [XmlElement("WorkingCurrentStepTime")]
    public string InternalWorkingCurrentStepTime
    {
      get
      {
        if (WorkingCurrentStepTime != null)
          return WorkingCurrentStepTime.Value.ToString();
        return "";
      }
      set { }
    }

    public LabeledThreadStatus(string label, int taskCount, TimeSpan? workingCurrentStepTime)
    {
      this.Label = label;
      this.TaskCount = taskCount;
      this.WorkingCurrentStepTime = workingCurrentStepTime;
    }
    public LabeledThreadStatus() { }
  }

  public class TaskPullStatus
  {
    public int ThreadCount = 0;
    public string MaxWorkingStepTime = "";
    public string[] HoverThreads = Array.Empty<string>();
    [XmlElement("Thread")]
    public LabeledThreadStatus[] Threads = Array.Empty<LabeledThreadStatus>();

    public TaskPullStatus() { }
    public TaskPullStatus(int threadCount, TimeSpan maxWorkingStepTime, string[] hoverThreads, LabeledThreadStatus[] threads)
    {
      this.ThreadCount = threadCount;
      this.MaxWorkingStepTime = maxWorkingStepTime.ToString();
      if (hoverThreads != null && hoverThreads.Length != 0)
        this.HoverThreads = hoverThreads;
      this.Threads = threads;
    }
  }

}
