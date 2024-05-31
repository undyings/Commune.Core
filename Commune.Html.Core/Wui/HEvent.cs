using Commune.Basis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroBolt.Wui
{
  public class HEventElement : HElement
  {
    public readonly hevent? handler = null;

    public HEventElement(HName name, params object[] content) :
      base(name, content)
    {
      foreach (object node in content)
      {
        if (node is hevent)
        {
          handler = (hevent)node;
          break;
        }
      }
    }
  }

  public class hevent : hdata
  {
    readonly Action<object[], JsonData> eventHandler;

    public void Execute(JsonData jsonData)
    {
      object[] ids = ArrayHlp.Convert(this.ToArray(),
        delegate(HObject attr)
        { 
          return ((HAttribute)attr).Value; 
        }
      );
      eventHandler(ids, jsonData);
    }

    public hevent(Action<object[], JsonData> eventHandler)
    {
      this.eventHandler = eventHandler;
    }
  }
}
