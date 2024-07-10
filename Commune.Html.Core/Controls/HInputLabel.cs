using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using NitroBolt.Wui;

namespace Commune.Html
{
	public class HInputLabel : ExtensionContainer, IHtmlControl
	{
		readonly string labelFor;
		readonly string caption;
		readonly HStyle[] pseudoClasses;
		public HInputLabel(string labelFor, object value, params HStyle[] pseudoClasses) :
			base("HInputLabel", "")
		{
			this.labelFor = labelFor;
			this.caption = value?.ToString() ?? "";
			this.pseudoClasses = pseudoClasses;
		}

		static readonly HBuilder h = HBuilder.Extension;

		public HElement ToHtml(string cssClassName, StringBuilder css)
		{
			//DefaultExtensionContainer defaults = new DefaultExtensionContainer(this);
			//defaults.InlineBlock();

			HtmlHlp.AddClassToCss(css, cssClassName, CssExtensions);

			foreach (HStyle pseudo in pseudoClasses)
				HtmlHlp.AddStyleToCss(css, cssClassName, pseudo);

			HtmlHlp.AddMediaToCss(css, cssClassName, MediaExtensions);

			HAttribute forAttr = new("for", labelFor);

			return new HElement("label", HtmlHlp.ContentForHElement(this, cssClassName, forAttr, caption));
		}
	}

	//public class HInputLabel : ExtensionContainer, IHtmlControl, IEventEditExtension
	//{
	//	readonly IHtmlControl input;
	//	readonly string caption;
	//	readonly HStyle[] pseudoClasses;
	//	public HInputLabel(string caption, IHtmlControl input, params HStyle[] pseudoClasses) :
	//		base("HInputLabel", "")
	//	{
	//		this.caption = caption;
	//		this.input = input;
	//		this.pseudoClasses = pseudoClasses;
	//	}

	//	static readonly HBuilder h = HBuilder.Extension;

	//	public HElement ToHtml(string cssClassName, StringBuilder css)
	//	{
	//		//DefaultExtensionContainer defaults = new DefaultExtensionContainer(this);
	//		//defaults.InlineBlock();

	//		HtmlHlp.AddClassToCss(css, cssClassName, CssExtensions);

	//		foreach (HStyle pseudo in pseudoClasses)
	//			HtmlHlp.AddStyleToCss(css, cssClassName, pseudo);

	//		HtmlHlp.AddMediaToCss(css, cssClassName, MediaExtensions);

	//		List<object> elements = new()
	//		{
	//			input.ToHtml(input.Name, css),
	//			caption
	//		};
	//		hdata? onevent = GetExtended("onevent") as hdata;
	//		if (onevent != null)
	//			elements.Add(onevent);

	//		//HElement inputElement = input.ToHtml(input.Name, css);

	//		return new HElement("label", HtmlHlp.ContentForHElement(this, cssClassName, elements.ToArray()));
	//	}
	//}
}
