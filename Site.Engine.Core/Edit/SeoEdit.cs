using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Basis;
using Commune.Html;
using Commune.Data;
//using System.Web.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Site.Engine
{
  public class SeoEdit
  {
		public static Func<EditState, JsonData[], RequestData, HtmlResult<HElement>> HViewCreator(HttpContext httpContext)
		{
			return delegate (EditState state, JsonData[] jsons, RequestData requestData)
			{
				foreach (JsonData json in jsons)
				{
					try
					{
						if (state.IsRattling(json))
							continue;

						state.Operation.Reset();

						HElement cachePage = Page(httpContext, state);

						hevent? eventh = cachePage.FindEvent(json, true);
						if (eventh != null)
						{
							eventh.Execute(json);
						}
					}
					catch (Exception ex)
					{
						Log.Error(ex, "");
						state.Operation.Error(string.Format("Непредвиденная ошибка: {0}", ex.Message));
					}
				}

				var page = Page(httpContext, state);
				return new HtmlResult<HElement>
				{
					Html = page,
					State = state,
					RefreshPeriod = TimeSpan.FromSeconds(5)
				};
			};
		}

    static readonly HBuilder h = HBuilder.Extension;

    static IHtmlControl GetCenterPanel(HttpContext httpContext, EditState state,
      string kind, int? parentId, int? id, out string title)
    {
      title = "";

      switch (kind)
      {
        case "group":
        case "fabric":
        case "page":
          return SeoEditorHlp.GetSEOObjectEdit(httpContext, state, kind, id, out title);
        //case "landing":
        //  return SeoEditorHlp.GetLandingEdit(state, id, out title);
        //case "landing-list":
        //  return SeoEditorHlp.GetLandingListEdit(state, out title);
        case "seo-pattern":
          return SeoEditorHlp.GetSEOPatternEdit(httpContext, state, out title);
        case "redirect":
          return SeoEditorHlp.GetRedirectEdit(httpContext, state, id, out title);
        case "redirect-list":
          return SeoEditorHlp.GetRedirectListEdit(out title);
        case "widget":
          return SeoEditorHlp.GetWidgetEdit(httpContext, state, id, out title);
        case "widget-list":
          return SeoEditorHlp.GetWidgetListEdit(httpContext, state, out title);
        default:
          return new HPanel();
      }
    }

    static HElement Page(HttpContext httpContext, EditState state)
    {
      int? parentId = httpContext.GetUInt("parent");
      string kind = httpContext.Get("kind");
      int? id = httpContext.GetUInt("id");
      if (id == null)
        id = state.CreatingObjectId;

      string title = "SEO поля";

      IHtmlControl editPanel = new HPanel();
      if (!httpContext.IsInRole("seo"))
      {
        editPanel = EditHlp.GetInfoMessage("Недостаточно прав для редактирования SEO полей", "/");
      }
      else if (state.Operation.Completed)
      {
        editPanel = EditHlp.GetInfoMessage(state.Operation.Message, state.Operation.ReturnUrl);
      }
      else
      {
        editPanel = GetCenterPanel(httpContext, state, kind, parentId, id, out title);
      }

      IHtmlControl mainPanel = new HPanel(
        new HPanel(
          editPanel.Background("white"),
          std.OperationWarning(state.Operation)
        ).WidthLimit("", "800px").Margin("0 auto")
      ).Width("100%").Background("#fafef9");

      StringBuilder css = new();

      std.AddStyleForFileUploaderButtons(css);

      HElement mainElement = mainPanel.ToHtml("main", css);

      return h.Html
      (
        h.Head(
          h.Element("title", title),
          h.LinkScript("/scripts/fileuploader.js"),
          h.LinkScript("/ckeditor/ckeditor.js"),
          HtmlHlp.CKEditorUpdateAll(),
          h.LinkCss("/css/fileuploader.css"),
          h.LinkCss("/css/static.css"),
          h.LinkCss("/css/font-awesome.css")
        ),
        h.Body(
          //HtmlHlp.CKEditorUpdateAll(),
          h.Css(h.Raw(css.ToString())),
          mainElement
        )
      );
    }
  }
}