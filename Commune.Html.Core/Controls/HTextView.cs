﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using NitroBolt.Wui;
using System.Drawing;

namespace Commune.Html
{
  public class HTextView : ExtensionContainer, IHtmlControl
  {
    readonly string text;
    readonly HStyle[] pseudoClasses;
    public HTextView(string text, params HStyle[] pseudoClasses) :
      base("HText", "")
    {
      this.text = text;
      this.pseudoClasses = pseudoClasses;
    }

    static readonly HBuilder h = HBuilder.Extension;

    public HElement ToHtml(string cssClassName, StringBuilder css)
    {
      HtmlHlp.AddClassToCss(css, cssClassName, CssExtensions);

      foreach (HStyle pseudo in pseudoClasses)
        HtmlHlp.AddStyleToCss(css, cssClassName, pseudo);

      HtmlHlp.AddMediaToCss(css, cssClassName, MediaExtensions);

      return h.Div(HtmlHlp.ContentForHElement(this, cssClassName, h.Raw(text))
      );
    }
  }
}
