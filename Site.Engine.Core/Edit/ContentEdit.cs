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
using Serilog;
using Microsoft.AspNetCore.Http;

namespace Site.Engine
{
	public class ContentEdit
  {
    public static Func<HttpContext, EditState, string, int?, int?, EnginePanelResult?>? GetCenterPanelExtension = null;
    public static DisplayName[] CatalogueSections = Array.Empty<DisplayName>();

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

      if (GetCenterPanelExtension != null)
      {
        EnginePanelResult? result = GetCenterPanelExtension(httpContext, state, kind, parentId, id);
        if (result != null)
        {
          title = result.Title;
          return result.Panel;
        }
      }

      switch (kind)
      {
        //case "group":
        //  return EditHlp.GetGroupEdit(state, parentId, id, out title);
        //case "fabric":
        //  return EditHlp.GetFabricEdit(state, parentId, id, out title);
        //case "variety":
        //  return EditHlp.GetVarietyEdit(state, parentId, id, out title);
        //case "fabric-feature":
        //  return FabricFeatureEditHlp.GetFeaturesView(state, id, out title);
        //case "payment":
        //  return EditHlp.GetPaymentWayEdit(state, id, out title);
        //case "delivery":
        //  return EditHlp.GetDeliveryWayEdit(state, id, out title);
        //case "news":
        //  return EditHlp.GetNewsEdit(state, id, out title);
        //case "sorting_payment":
        //  return EditHlp.GetSortingPaymentWays(state, out title);
        //case "sorting_delivery":
        //  return EditHlp.GetSortingDeliveryWays(state, out title);
        //case "sorting_group":
        //  return EditHlp.GetSortingGroupEdit(state, parentId, out title);
        //case "sorting_fabric":
        //  return EditHlp.GetSortingFabricEdit(state, parentId, out title);
        case "sorting_section":
          return EditHlp.GetSortingSectionEdit(httpContext, state, parentId, out title);
        //case "sorting_unit":
        //  return EditHlp.GetSortingUnitEdit(state, parentId, out title);
        case "sorting_subunit":
          return EditHlp.GetSortingSubunitEdit(httpContext, state, parentId, out title);
        case "page":
          {
            string design = httpContext.Get("design");
            return EditHlp.GetSectionEdit(httpContext, state, parentId, id, design, out title);
          }
        case "unit":
          {
            string design = httpContext.Get("design");
            return EditHlp.GetUnitEdit(httpContext, state, parentId, id, design, out title);
          }
        //case "oplata-i-dostavka":
        //case "offers":
        //  return EditHlp.GetArticleEdit(httpContext, state, kind, out title);
        //case "kontakty":
        //  return EditHlp.GetContactsViewEdit(httpContext, state, out title);
        case "contacts-column":
          return EditHlp.GetContactsColumnEdit(httpContext, state, out title);
        case "catalogue":
          return EditHlp.GetCatalogueEdit(state, ContentEdit.CatalogueSections, out title);
        //case "kind-list":
        //  return MetaEditHlp.GetKindListEdit(out title);
        //case "kind":
        //  return MetaEditHlp.GetKindEdit(state, id, out title);
        //case "property-list":
        //  return MetaEditHlp.GetPropertyListEdit(out title);
        //case "property":
        //  return MetaEditHlp.GetPropertyEdit(state, id, out title);
        //case "category-list":
        //  return MetaEditHlp.GetCategoryListEdit(out title);
        //case "category":
        //  return MetaEditHlp.GetCategoryEdit(state, id, out title);
        //case "feature-list":
        //  return FeatureEditHlp.GetFeatureListEdit(state, out title);
        //case "feature":
        //  return FeatureEditHlp.GetFeatureEdit(state, id, out title);
        //case "feature-value":
        //  return FeatureEditHlp.GetFeatureValueEdit(state, parentId, id, out title);
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

      string title = "Редактирование";

      IHtmlControl editPanel = new HPanel();
      if (!httpContext.IsInRole("edit"))
      {
        editPanel = EditHlp.GetInfoMessage("Недостаточно прав для редактирования", "/");
      }
      else if (state.Operation.Completed)
      {
        editPanel = EditHlp.GetInfoMessage(state.Operation.Message, state.Operation.ReturnUrl);
      }
      else
      {
        editPanel = GetCenterPanel(httpContext, state, kind, parentId, id, out title);
      }

      HEventPanel mainPanel = new HEventPanel(
        new HPanel(
          editPanel.Background("white"),
          EditElementHlp.GetOperationPopup(state.Operation)
          //std.OperationWarning(state.Operation)
        ).WidthLimit("", "800px").Margin("0 auto")
      ).Width("100%").Background("#fafef9");

      if (state.Operation.Status != null)
      {
        mainPanel.OnClick(";").Event("main_popup_reset", "",
          delegate
          {
            state.Operation.Reset();
          }
        );
      }

      StringBuilder css = new StringBuilder();

      std.AddStyleForFileUploaderButtons(css);

      HElement mainElement = mainPanel.ToHtml("main", css);

			return h.Html
			(
				h.Head(
					h.Element("title", title),
					h.LinkScript("/scripts/fileuploader.js"),
					h.LinkScript("/ckeditor/ckeditor.js?v=4113"),
					//h.LinkScript("https://cdn.ckeditor.com/4.11.3/full-all/ckeditor.js"),
					HtmlHlp.CKEditorUpdateAll(),
          h.LinkCss("/css/fileuploader.css"),
          h.LinkCss("/css/static.css")
          //h.LinkCss("/css/font-awesome.css")
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