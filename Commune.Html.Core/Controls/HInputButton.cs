using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using NitroBolt.Wui;

namespace Commune.Html
{
  public class HInputButton : ExtensionContainer, IHtmlControl, IEventEditExtension
  {
    readonly string caption;
    readonly HStyle[] pseudoClasses;
    public HInputButton(string caption, HStyle[] pseudoClasses) :
      base("HInputButton", "")
    {
      this.caption = caption;
      this.pseudoClasses = pseudoClasses;
    }

    static readonly HBuilder h = HBuilder.Extension;

    public HElement ToHtml(string cssClassName, StringBuilder css)
    {
      HtmlHlp.AddClassToCss(css, cssClassName, CssExtensions);

      foreach (HStyle pseudo in pseudoClasses)
        HtmlHlp.AddStyleToCss(css, cssClassName, pseudo);

      List<object> elements = new List<object>();

      elements.Add(h.type("button"));
      elements.Add(h.value(caption));

      hdata? onevent = GetExtended("onevent") as hdata;
      if (onevent != null)
        elements.Add(onevent);

      DefaultExtensionContainer defaults = new DefaultExtensionContainer(this);
      defaults.OnClick(";");
      defaults.Cursor(CursorStyle.Pointer);

      return new HElement("input", HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray())
      );

      //return h.Input(HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray())
      //);
    }
  }
}
