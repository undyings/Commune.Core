using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using NitroBolt.Wui;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Commune.Html
{
  public class HInputCheck : ExtensionContainer, IHtmlControl
  {
    readonly bool isChecked;
    readonly HStyle[] pseudoClasses;
    readonly Action<JsonData>? eventHandler;

		public HInputCheck(string dataName, bool isChecked, Action<JsonData>? eventHandler, params HStyle[] pseudoClasses) :
      base("HInputCheck", dataName)
    {
      this.isChecked = isChecked;
      this.eventHandler = eventHandler;
      this.pseudoClasses = pseudoClasses;
    }

    public HInputCheck(string dataName, bool isChecked, params HStyle[] pseudoClasses) :
      this(dataName, isChecked, null, pseudoClasses)
    {
    }

		static readonly HBuilder h = HBuilder.Extension;

    public HElement ToHtml(string cssClassName, StringBuilder css)
    {
      {
        DefaultExtensionContainer defaults = new DefaultExtensionContainer(this);
        defaults.Display("inline-block");
        if (eventHandler != null)
					defaults.OnClick(";");
			}

      HtmlHlp.AddClassToCss(css, cssClassName, CssExtensions);
      foreach (HStyle pseudo in pseudoClasses)
        HtmlHlp.AddStyleToCss(css, cssClassName, pseudo);

      List<object> elements = new();
      elements.Add(h.type("checkbox"));
      elements.Add(h.data("name", Name));
      elements.Add(h.data("id", Name));
			elements.Add(new HAttribute("id", Name));
			if (isChecked)
        elements.Add(h.@checked());

			if (eventHandler != null)
			{
				hevent onevent = HtmlExt.InnerEvent(Name, "", eventHandler);
				elements.Add(onevent);

				return new HEventElement("input", HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray()));
			}

			return h.Input(HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray())
      );
    }
  }
}
