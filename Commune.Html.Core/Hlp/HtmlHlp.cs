﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Basis;
using System.Web;
using Serilog;
using Microsoft.AspNetCore.Http;

namespace Commune.Html
{
  public static class HtmlHlp
  {
    static readonly HBuilder h = HBuilder.Extension;

    public static HSpan VAligner()
    {
      return new HSpan("").InlineBlock().Height("100%").VAlign(null);
    }

    public static IHtmlControl PagingKeyboard()
    {
      return new HElementControl(h.Script(h.Raw(@"
window.addEventListener('keydown', function(e) 
{
 if (e.keyCode == 37) { var b = $('.prev'); if (b.length > 0) b[0].click(); }
 else if (e.keyCode == 39) { var b = $('.next'); if (b.length > 0) b[0].click(); }
});
")), "pagingKeyboard_script");
    }

    public static IHtmlControl UpbuttonScript(int hideScrollTop, int staticScrollBottom)
    {
      string scriptPattern = @"
if (window.upbuttonLaunch != true)
{
  window.upbuttonLaunch = true;
  window.onscroll = function() {
    var scrolled = window.pageYOffset || document.documentElement.scrollTop;
    var clientHeight = document.documentElement.clientHeight;
  
    var scrollHeight = Math.max(
      document.body.scrollHeight, document.documentElement.scrollHeight,
      document.body.offsetHeight, document.documentElement.offsetHeight,
      document.body.clientHeight, document.documentElement.clientHeight
    );
    if (scrolled < <<scrollTop>>) {
      $('.upbutton')[0].style.display = 'none';
    }
    else {
      $('.upbutton')[0].style.display = 'inline-block';
      if (scrolled + clientHeight < scrollHeight - <<scrollBottom>>)
        $('.upbutton')[0].style.position = 'fixed';
      else
        $('.upbutton')[0].style.position = 'static';
    }
  }
}
";

      return new HElementControl(h.Script(h.Raw(
        scriptPattern.Replace("<<scrollTop>>", hideScrollTop.ToString())
          .Replace("<<scrollBottom>>", staticScrollBottom.ToString())
        )), "upbutton_script"
      );
    }

    public static HtmlResult FirstHtmlResult(HElement page, object state, TimeSpan? refreshPeriod)
    {
      return new HtmlResult
      {
        Html = page,
        FirstHtmlTransformer = HtmlHlp.FirstHtmlTransformer,
        ToHtmlText = HtmlHlp.ToHtmlText,
        State = state,
        RefreshPeriod = refreshPeriod
      };
    }

    public static HElement FirstHtmlTransformer(HElement element)
    {
      return element;
    }

    public static string ToHtmlText(HElement element)
    {
      StringBuilder builder = new StringBuilder();
      builder.Append("<!DOCTYPE html>");
      builder.Append("<html>");
      ToHtmlText(builder, (HElement)element.Nodes[0], false);
      ToHtmlText(builder, (HElement)element.Nodes[1], true);
      builder.Append("</html>");
      return builder.ToString();
    }

    //public static string ToHtmlText(HElement element)
    //{
    //  StringBuilder builder = new StringBuilder();
    //  ToHtmlText(builder, element);
    //  return builder.ToString();
    //}

    static void ToHtmlText(StringBuilder builder, HElement element, bool isBody)
    {
      string elementType = element.Name.LocalName?.ToString() ?? "";
      builder.Append("<");
      builder.Append(elementType);
      foreach (var attribute in element.Attributes)
      {
        builder.Append(' ');
        attribute.ToHtmlText(builder);
      }
      if (IsEmptyHtmlElementType(elementType))
        builder.Append("/>");
      else
      {
        if (!element.Nodes.Any())
          builder.Append(">");
        else
        {
          builder.Append(">");
          foreach (var node in element.Nodes)
          {
            if (node is HElement)
            {
              HElement elem = (HElement)node;
              if (isBody && elem.Name.LocalName == "script")
              {
                continue;
                //Logger.AddMessage("Script: {0}", elem.ToHtmlText());
              }
              ToHtmlText(builder, elem, isBody);
            }
            else if (node is HText)
              builder.Append(HttpUtility.HtmlEncode(((HText)node).Text));
            else
              node.ToHtmlText(builder);
          }
        }
        builder.Append("</");
        builder.Append(elementType);
        builder.Append(">");
      }

    }

    static bool IsEmptyHtmlElementType(string elementType)
    {
      switch (elementType)
      {
        case "area":
        case "base":
        case "br":
        case "col":
        case "command":
        case "embed":
        case "hr":
        case "img":
        case "input":
        case "keygen":
        case "link":
        case "meta":
        case "param":
        case "source":
        case "track":
        case "wbr":
          return true;
        default:
          return false;
      }
    }

    static TimeSpan? ParseTimezone(string browserTime)
    {
      if (StringHlp.IsEmpty(browserTime))
        return null;

      int index = browserTime.ToUpper().IndexOf("GMT+");
      if (index < 0)
        return null;

      if (index + 8 > browserTime.Length)
        return null;

      ushort rawTimezone;
      if (!ushort.TryParse(browserTime.Substring(index + 4, 4), out rawTimezone))
        return null;

      return new TimeSpan(rawTimezone / 100, rawTimezone % 100, 0);
    }

    public static IHtmlControl TimezoneScriptControl(WuiInitiator initiator, Action<TimeSpan?> timezoneSetter)
    {
      return new HPanel(
        new HTextEdit("timezone"),
        new HButton("timezone_btn", "timezone")
          .Event(initiator, "timezone_command", "timezone_content", delegate (JsonData json)
          {
            string rawTimezone = json.GetText("timezone");
            TimeSpan? timezone = ParseTimezone(rawTimezone);
            if (timezone == null)
              Log.Information("Ошибка парсинга timezone: '{0}'", rawTimezone);
            timezoneSetter(timezone);
          }),
        new HElementControl(
          h.Script(h.Raw("$(function () {{ $('.timezone').val(new Date()); }}); $('.timezone_btn').click();")),
          "timezone_script"
        )
      ).EditContainer("timezone_content").Display("none");
    }

    public static HElement DiapasonSlider(string cssClassName, float min, float max,
      float value1, float value2)
    {
      return h.Script(h.Raw(string.Format(@"
        $('#{0}').slider({{
          range: true,
          min: {1},
          max: {2},
          values: [{3}, {4}],
          slide: function( event, ui ) {{ ;
            }}
        }});",
        cssClassName, min, max, value1, value2
      )));
    }

    public static HElement? RedirectScript(string redirectUrl)
    {
      if (StringHlp.IsEmpty(redirectUrl))
        return null;

      return h.Script(h.type("text/javascript"), string.Format(@"$(location).attr('href','{0}');", redirectUrl));
    }

		public static IHtmlControl CKEditorCreate(string dataName, string text,
			string height, bool isRu, params string[] editorProps)
		{
			List<string> propList = new List<string>();
			propList.Add(string.Format("height: '{0}'", height));
			//propList.Add(string.Format("extraPlugins: 'justify,colorbutton,indentblock,textselection'"));
			propList.Add("allowedContent: true");
			if (isRu)
				propList.Add("language: 'ru'");
			propList.AddRange(editorProps);

			return new HElementControl(
				h.TextArea(
					new HAttribute("id", dataName),
					h.@class(dataName),
					h.data("name", dataName),
					h.Attribute("js-init", string.Format("CKEDITOR.replace(this, {{ {0} }})",
						StringHlp.Join(", ", "{0}", propList)
					)),
					text
				), dataName
			);
		}

		public static HElement CKEditorUpdateAll()
    {
      return h.Script(@"
               function CK_updateAll()
               {
                 try
                 {
                   for (instance in CKEDITOR.instances)
                     CKEDITOR.instances[instance].updateElement();
                 }
                 catch(ex)
                 {
                   console.log(ex);
                 }
               }
            ");
    }

    public static HElement? SchemaOrg(SchemaOrg schema)
    {
      if (schema == null)
        return null;

      return h.Script(h.type("application/ld+json"),
        h.Raw(string.Format(
@"{{
  ""@context"": ""http://schema.org"",
  ""@type"": ""{0}"",
  ""mainEntityOfPage"": {{
    ""@type"": ""WebPage"",
    ""@id"": ""{1}""
  }},
  ""headline"": ""{2}"",
  ""image"": [ {3} ],
  ""datePublished"": ""{4:O}"",
  ""dateModified"": ""{5:O}"",
  ""author"": {{
    ""@type"": ""Person"",
    ""name"": ""{6}""
  }},
  ""publisher"": {{
    ""@type"": ""Organization"",
    ""name"": ""{7}"",
    ""logo"": {{
      ""@type"": ""ImageObject"",
      ""url"": ""{8}""
    }}
  }},
  ""description"": ""{9}"",
  ""articleBody"": """"
}}
", 
          schema.PageType, schema.PageUrl, schema.Title, StringHlp.Join(", ", @"""{0}""", schema.ImageUrls),
          schema.PublishedTime.ToLocalTime(), schema.ModifiedTime.ToLocalTime(),
          schema.Author, schema.Organization, schema.LogoUrl, schema.Description
        ))
      );
    }

    public static HElement GoogleAnalytics()
    {
      return h.Script(h.type("text/javascript"), h.src("http://www.google-analytics.com/ga.js"), "");
    }

    public static HElement YandexMetrics()
    {
      return h.Script(h.type("text/javascript"), h.src("https://mc.yandex.ru/metrika/watch.js"), "");
    }

    public static T CKEditorOnUpdateAll<T>(this T control) where T : IEditExtension
    {
      return control.OnClick("CK_updateAll();");
    }

    public static string Get(this HttpContext context, string argName)
    {
      string? arg = context.Request.Query[argName];
      if (arg == null || arg == "")
        return "";
      return arg.ToLower();
    }

    public static int? GetUInt(this HttpContext context, string argName)
    {
      return Parse(context.Request.Query[argName]);
    }

    public static int? Parse(string? arg)
    {
      if (StringHlp.IsEmpty(arg))
        return null;

      int value;
      if (int.TryParse(arg, out value) && value >= 0)
        return value;
      return null;
    }

    public static string ColorToHtml(Color color)
    {
      return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
    }

    public static object[] ContentForHElement(IHtmlControl control, string cssClassName, params object[] coreAttrs)
    {
      HAttribute classAttr;
      {
        string[] extraClassNames = (control.GetExtended("extraClassNames") as string[]) ?? Array.Empty<string>();
        if (extraClassNames.Length == 0)
          classAttr = h.@class(cssClassName);
        else
        {
          string[] classNames = ArrayHlp.Merge(extraClassNames, new string[] { cssClassName });
          classAttr = h.@class(classNames);
        }
      }
        
      List<object> content = new() { classAttr };
      content.AddRange(coreAttrs);
      foreach (TagExtensionAttribute extension in control.TagExtensions)
        content.Add(new HAttribute(extension.Name, extension.Value));
      return content.ToArray();
    }

    //public static void AddHoverToCss(StringBuilder css, string cssClassName, IReadExtension control)
    //{
    //  AddPseudoClassToCss(css, "hover", cssClassName, control);
    //}

    public static void AddKeyFramesToCss(StringBuilder css, string framesName, params HStyle[] frames)
    {
      css.AppendLine(string.Format("@keyframes {0} {{", framesName));
      foreach (HStyle frame in frames)
      {
        css.Append(frame.Name);
        css.Append(" {");
        foreach (CssExtensionAttribute extension in frame.CssExtensions)
          css.AppendFormat(" {0}:{1};", extension.Name, extension.Value);
        css.Append("}");
        css.Append(Environment.NewLine);
      }
      css.AppendLine("}");
    }

    public static void AddClassToCss(StringBuilder css, string className, 
      IEnumerable<CssExtensionAttribute> cssExtensions)
    {
      if (!_.CountMore(cssExtensions, 0))
        return;

      css.AppendLine(string.Format(".{0} {{", className));
      foreach (CssExtensionAttribute extension in cssExtensions)
        css.AppendLine(string.Format("  {0}:{1};", extension.Name, extension.Value));
      css.AppendLine("}");
    }

    public static void AddStyleToCss(StringBuilder css, string cssClassName, HStyle style)
    {
      if (style == null)
        return;

      AddExtensionsToCss(css, style.CssExtensions, style.Name, cssClassName);
    }

    public static void AddExtensionsToCss(StringBuilder css,
      IEnumerable<CssExtensionAttribute> cssExtensions,
      string cssPrefixFormat, params string[] cssClassNames)
    {
      if (!_.CountMore(cssExtensions, 0))
        return;

      css.AppendLine(string.Format(cssPrefixFormat, cssClassNames) + " {");
      foreach (CssExtensionAttribute extension in cssExtensions)
        css.AppendLine(string.Format("  {0}:{1};", extension.Name, extension.Value));
      css.AppendLine("}");
    }

    public static void AddMediaToCss(StringBuilder css, string cssClassName,
      IEnumerable<MediaExtensionAttribute> mediaExtensions)
    {
      foreach (MediaExtensionAttribute media in mediaExtensions)
      {
        HStyle[] styles = media.Styles;
        if (styles == null || styles.Length == 0)
          continue;

        string mediaPseudo = string.Format(media.Name, cssClassName);

        css.AppendLine(string.Format("@media {0} {{", mediaPseudo));
        foreach (HStyle style in styles)
        {
          AddStyleToCss(css, cssClassName, style); 
        }
        css.AppendLine("}");
      }
    }

    public static string BreakLongestWords(string plainText)
    {
      return BreakLongestWords(plainText, 30);
    }

    public static string BreakLongestWords(string plainText, int maxWordLength)
    {
      if (plainText == null)
        return "";

      Stack<int> delimiters = new Stack<int>();
      int lastSpace = 0;
      for (int i = 0; i < plainText.Length; ++i)
      {
        if (char.IsWhiteSpace(plainText[i]))
        {
          lastSpace = i;
          continue;
        }

        if (i - lastSpace > maxWordLength)
        {
          delimiters.Push(i);
          lastSpace = i;
          continue;
        }
      }

      if (delimiters.Count == 0)
        return plainText;

      List<char> chars = new List<char>(plainText.Length + delimiters.Count);
      chars.AddRange(plainText);
      while (delimiters.Count > 0)
      {
        int delimiter = delimiters.Pop();
        chars.Insert(delimiter, ' ');
      }

      return new string(chars.ToArray());
    }

  //  public static void AddPseudoClassToCss(StringBuilder css,
  //    string pseudoClassType, string cssClassName, IReadExtension control)
  //  {
  //    HStyle pseudoClass = control.GetExtended(pseudoClassType) as HStyle;
  //    if (pseudoClass == null)
  //      return;

  //    if (!_.CountMore(pseudoClass.CssExtensions, 0))
  //      return;

  //    css.AppendLine(string.Format(pseudoClass.Name + "{{", cssClassName));
  //    foreach (CssExtensionAttribute extension in pseudoClass.CssExtensions)
  //      css.AppendLine(string.Format("  {0}:{1};", extension.Name, extension.Value));
  //    css.AppendLine("}");
  //  }
  }
}
