﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using NitroBolt.Wui;

namespace Commune.Html
{
  public class HInputRadio : ExtensionContainer, IHtmlControl, IEventEditExtension
  {
    readonly string groupName;
    //readonly object objectId;
    readonly bool isChecked;
    //readonly Action<JsonData>? eventHandler;
    readonly HStyle[] pseudoClasses;

    public HInputRadio(string groupName, object objectId, bool isChecked, params HStyle[] pseudoClasses) :
      base("HInputRadio", string.Format("{0}_{1}", groupName, objectId))
    {
      this.groupName = groupName;
      //this.objectId = objectId;
      this.isChecked = isChecked;
      //this.eventHandler = eventHandler;
      this.pseudoClasses = pseudoClasses;
		}

    //public HInputRadio(string groupName, object objectId, bool isChecked, params HStyle[] pseudoClasses) :
    //    this(groupName, objectId, isChecked, null, pseudoClasses)
    //{
    //}

    static readonly HBuilder h = HBuilder.Extension;

    public HElement ToHtml(string cssClassName, StringBuilder css)
    {
      {
        DefaultExtensionContainer defaults = new(this);
        defaults.Display("inline-block");
        defaults.OnClick(";");
      }

      HtmlHlp.AddClassToCss(css, cssClassName, CssExtensions);
      foreach (HStyle pseudo in pseudoClasses)
        HtmlHlp.AddStyleToCss(css, cssClassName, pseudo);

      List<object> elements = new() 
      {
				h.type("radio"),
        new HAttribute("id", Name),
				new HAttribute("name", groupName),
				h.data("name", Name),
				h.data("id", Name)
			};
      if (isChecked)
        elements.Add(h.@checked());

			hdata? onevent = GetExtended("onevent") as hdata;
			if (onevent != null)
				elements.Add(onevent);

			//if (eventHandler != null)
			//{
			//  hdata onevent = HtmlExt.InnerEvent(groupName, "", objectId);
			//  elements.Add(onevent);

			//  return h.Input(HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray()));
			//}

			//elements.Add(h.value(objectId));
			//hevent onevent = GetExtended("onevent") as hevent;
			//if (onevent != null)
			//{
			//  elements.Add(onevent);

			//  return new HEventElement("input", HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray()));
			//}

			return h.Input(HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray()));
    }
  }
}
