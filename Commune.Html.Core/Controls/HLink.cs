using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using NitroBolt.Wui;

namespace Commune.Html
{
  public class HLink : ExtensionContainer, IHtmlControl
  {
    readonly object linkObject;
    //readonly IHtmlControl innerControl;
    readonly string url;
    readonly HStyle[] pseudoClasses;

    public HAttribute[] AdditionalAttributes = Array.Empty<HAttribute>();

    public HLink(string url, string caption, params HStyle[] pseudoClasses) :
      base("HLink", "")
    {
      this.url = url;
      this.linkObject = caption;
      this.pseudoClasses = pseudoClasses;
    }

    public HLink(string url, IHtmlControl innerControl, params HStyle[] pseudoClasses) :
      base("HLink", "")
    {
      this.url = url;
      this.linkObject = innerControl;
      this.pseudoClasses = pseudoClasses;
    }

		//public HLink(string url, HObject innerElement, params HStyle[] pseudoClasses) :
		//	base("HLink", "")
		//{
		//	this.url = url;
		//	this.linkObject = innerElement;
		//	this.pseudoClasses = pseudoClasses;
		//}

		static readonly HBuilder h = HBuilder.Extension;

    public HElement ToHtml(string cssClassName, StringBuilder css)
    {
      HtmlHlp.AddClassToCss(css, cssClassName, CssExtensions);

      foreach (HStyle pseudo in pseudoClasses)
        HtmlHlp.AddStyleToCss(css, cssClassName, pseudo);

      HtmlHlp.AddMediaToCss(css, cssClassName, MediaExtensions);

			List<object> elements = new()
			{
				h.href(url)
			};

      elements.AddRange(AdditionalAttributes);

			if (linkObject is IHtmlControl control)
      {
        HElement innerElement = control.ToHtml(string.Format("{0}_inner", cssClassName), css);

        elements.Add(innerElement);
      }
      //else if (linkObject is HObject element)
      //{
      //  elements.Add(element);
      //}
      else
      {
        elements.Add(linkObject);
      }

      return h.A(HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray())
      );
    }
  }
}
