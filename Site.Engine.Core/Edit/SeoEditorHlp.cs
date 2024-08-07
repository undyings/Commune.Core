﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Drawing;
using System.IO;
using Commune.Html;
using NitroBolt.Wui;
using Commune.Data;
using Commune.Basis;
using Microsoft.AspNetCore.Http;

namespace Site.Engine
{
  public class SeoEditorHlp
  {
    static IStore store
    {
      get
      {
        return BaseContext.Default.Store;
      }
    }

    static DataLayer fabricConnection
    {
      get
      {
        return BaseContext.Default.FabricConnection;
      }
    }

    public static IHtmlControl GetWidgetListEdit(HttpContext httpContext, WuiInitiator initiator, EditState state, out string title)
    {
      title = "SEO виджеты";

      ObjectBox widgetBox = store.SeoWidgets.widgetBox;

      string robotsText = "";
      string robotsPath = Path.Combine(BaseContext.Default.WebRootPath, "robots.txt");
      if (File.Exists(robotsPath))
        robotsText = File.ReadAllText(robotsPath, Encoding.UTF8);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
					new HPanel(
						widgetBox.AllObjectIds.Select(widgetId =>
						{
							LightObject widget = new(widgetBox, widgetId);

							return new HPanel(
                DecorEdit.RedoIconButton(true, UrlHlp.SeoUrl("widget", widgetId)).VAlign(-2).MarginRight(10),
                new HLabel(widget.Get(SEOWidgetType.DisplayName).Name)
              ).Padding(5, 0);
            }).ToArray()
					).MarginTop(10),
					new HPanel(
            DecorEdit.RedoButton(true, "Добавить виджет", UrlHlp.SeoUrl("widget", null))
          ),
          new HPanel(
            DecorEdit.FieldArea("robots.txt",
              new HTextArea("robotsText", robotsText).Height("12em")
            ),
            new HPanel(
              std.Button("Сохранить robots.txt").MarginTop(10)
                .Event(initiator, "robots_save", "editRobots",
                  delegate(JsonData json)
                  {
                    if (httpContext.IsInRole("nosave"))
                    {
                      state.Operation.Warning("Нет прав на сохранение изменений");
                      return;
                    }

                    string editRobotsText = json.GetText("robotsText");
                    File.WriteAllText(robotsPath, editRobotsText);
                  }
                )
            )
          ).MarginTop(20).EditContainer("editRobots")
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.ReturnButton("/")
        )
      );
    }

    public static IHtmlControl GetWidgetEdit(HttpContext httpContext, WuiInitiator initiator, EditState state, int? widgetId, out string title)
    {
			if (widgetId == null)
        widgetId = state.CreatingObjectId;

      title = widgetId == null ? "Добавить виджет" : "Редактировать виджет";

      ObjectBox widgetBox = store.SeoWidgets.widgetBox;

      if (widgetId != null && !widgetBox.ObjectById.Exist(widgetId.Value))
        return EditHlp.GetInfoMessage("Неверный аргумент", "/");

      LightObject? widget = widgetId != null ? new LightObject(widgetBox, widgetId.Value) : null;

      string returnUrl = UrlHlp.SeoUrl("widget-list", null);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field("Наименование", "name", widget != null ? widget.Get(SEOWidgetType.DisplayName).Name : ""),
          DecorEdit.FieldArea("Код виджета", 
            new HTextArea("code", widget?.Get(SEOWidgetType.Code) ?? "").Height("16em")
          ),
          EditElementHlp.GetDeletePanel(httpContext, initiator, state, widgetId ?? -1, 
            "виджет", "Удаление виджета", null
          ).Hide(widget == null)
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          (widget != null ? DecorEdit.SaveButton() : DecorEdit.AddButton("Добавить"))
          .Event(initiator, "save_widget", "editContent",
            delegate (JsonData json)
            {
              string editName = json.GetText("name");
              string editCode = json.GetText("code");

              if (StringHlp.IsEmpty(editName))
              {
                state.Operation.Warning("Не задано найменование виджета");
                return;
              }
              if (StringHlp.IsEmpty(editCode))
              {
                state.Operation.Warning("Не задан код виджета");
                return;
              }

              if (httpContext.IsInRole("nosave"))
              {
                state.Operation.Warning("Нет прав на сохранение изменений");
                return;
              }

              using BoxDbContext dbContext = fabricConnection.Create();

              LightObject? editWidget;
              if (widgetId != null)
              {
                editWidget = DataBox.LoadObject(fabricConnection, dbContext, widgetId.Value);

                if (editWidget != null && !SEOWidgetType.DisplayName.SetWithCheck(dbContext, editWidget.Box, editWidget.Head, new NameId(editName)))
                {
                  state.Operation.Warning("Другой виджет с таким наименованием уже существует");
                  return;
                }
              }
              else
              {
                ObjectBox box = new ObjectBox(fabricConnection, dbContext);
                int? createWidgetId = box.CreateUniqueObject(dbContext, 
                  SEOWidgetType.Widget, SEOWidgetType.DisplayName.Create(new NameId(editName)), null);

                if (createWidgetId == null)
                {
                  state.Operation.Warning("Виджет с таким наименованием уже существует");
                  return;
                }

                editWidget = new LightObject(box, createWidgetId.Value);

                state.CreatingObjectId = editWidget.Id;
              }

              if (editWidget == null)
                return;

              editWidget.Set(SEOWidgetType.Code, editCode);

              editWidget.Box.Update(dbContext, true);

              BaseContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    public static IHtmlControl GetRedirectListEdit(out string title)
    {
      title = "Все перенаправления";

      List<IHtmlControl> elements = new List<IHtmlControl>();

      elements.Add(
        new HPanel(
          DecorEdit.RedoButton(true, "Добавить ссылку", UrlHlp.SeoUrl("redirect", null))
        ).MarginTop(0).MarginBottom(10)
      );

      foreach (LightObject redirect in store.Redirects.All)
      {
        string from = redirect.Get(RedirectType.From).Name;
        string to = redirect.Get(RedirectType.To);

        HPanel redirectPanel = new HPanel(
          new HPanel(
            DecorEdit.RedoIconButton(true, UrlHlp.SeoUrl("redirect", redirect.Id)).VAlign(-2).MarginRight(10),
            new HLink(from,
              new HLabel(from)
            )
          ).RelativeWidth(50).VAlign(null),
          new HPanel(
            new HButton("", std.BeforeAwesome(@"\f178", 0)).MarginRight(10),
            new HLabel(to)
          ).RelativeWidth(50)
        ).MarginBottom(6);

        elements.Add(redirectPanel);
      }

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          elements.ToArray()
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.ReturnButton("/")
        )
      );
    }

    public static IHtmlControl GetRedirectEdit(HttpContext httpContext, WuiInitiator initiator, EditState state, int? redirectId, out string title)
    {
      if (redirectId == null)
        redirectId = state.CreatingObjectId;

      title = redirectId == null ? "Добавить перенаправление" : "Редактировать перенаправление";

      LightObject? redirectLink = store.Redirects.Find(redirectId);

      if (redirectId != null && redirectLink == null)
        return EditHlp.GetInfoMessage("Неверный аргумент", "/");

      string returnUrl = UrlHlp.SeoUrl("redirect-list", null);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field("Перенаправляемый адрес", "from", redirectLink != null ? redirectLink.Get(RedirectType.From).Name : ""),
          DecorEdit.Field("Перенаправить по адресу", "to", redirectLink != null ? redirectLink.Get(RedirectType.To) : ""),
          EditElementHlp.GetDeletePanel(httpContext, initiator, state, redirectId ?? -1, 
            "перенаправление", "Удаление перенаправления", null
          ).Hide(redirectId == null)
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          (redirectLink != null ? DecorEdit.SaveButton() : DecorEdit.AddButton("Добавить"))
          .Event(initiator, "save_deadlink", "editContent",
            delegate (JsonData json)
            {
              string editFrom = json.GetText("from").ToLower();
              string editTo = json.GetText("to").ToLower();

              if (StringHlp.IsEmpty(editFrom))
              {
                state.Operation.Warning("Не задан перенаправляемый адрес");
                return;
              }
              if (StringHlp.IsEmpty(editTo))
              {
                state.Operation.Warning("Не задано куда перенаправлять");
                return;
              }

              if (httpContext.IsInRole("nosave"))
              {
                state.Operation.Warning("Нет прав на сохранение изменений");
                return;
              }

              using BoxDbContext dbContext = fabricConnection.Create();

              LightObject? editRedirect = null;
              if (redirectId != null)
              {
                editRedirect = DataBox.LoadObject(fabricConnection, dbContext, redirectId.Value);

                if (editRedirect != null && !RedirectType.From.SetWithCheck(dbContext, editRedirect.Box, editRedirect.Head, new NameId(editFrom)))
                {
                  state.Operation.Warning("Другое перенаправление с такого адреса уже существует");
                  return;
                }
              }
              else
              {
                ObjectBox box = new(fabricConnection, dbContext);
                int? createRedirectId = box.CreateUniqueObject(dbContext, 
                  RedirectType.Redirect, RedirectType.From.Create(new NameId(editFrom)), null);

                //Logger.AddMessage("Redirect.Edit: {0}, {1}", editFrom, editTo);

                if (createRedirectId == null)
                {
                  state.Operation.Warning("Перенаправление с такого адреса уже существует");
                  return;
                }

                editRedirect = new LightObject(box, createRedirectId.Value);

                state.CreatingObjectId = editRedirect.Id;
              }

							if (editRedirect == null)
								return;

							editRedirect.Set(RedirectType.To, editTo);

              editRedirect.Box.Update(dbContext, true);

              BaseContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    //static IHtmlControl GetLandingAdd(ShopStorage shop, EditState state, out string title)
    //{
    //  title = "Добавление посадочной страницы";

    //  return new HPanel(
    //    DecorEdit.Title(title),
    //    new HPanel(
    //      DecorEdit.Field("H1 заголовок", "landingName", ""),
    //      EditElementHlp.GetFabricKindCombo(shop, null)
    //    ).Margin(0, 10).MarginBottom(20),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.AddButton("Добавить")
    //      .Event("add_landing", "editContent",
    //        delegate (JsonData json)
    //        {
    //          string landingName = json.GetText("landingName");
    //          string fabricKind = json.GetText("fabricKind");
    //          if (StringHlp.IsEmpty(landingName))
    //          {
    //            state.Operation.Message = "Не задан заголовок посадочной страницы";
    //            return;
    //          }

    //          if (HttpContext.Current.IsInRole("nosave"))
    //          {
    //            state.Operation.Message = "Нет прав на сохранение изменений";
    //            return;
    //          }

    //          ObjectBox box = new ObjectBox(fabricConnection, "1=0");
    //          int? createLandingId = box.CreateUniqueObject(LandingType.Landing,
    //            LandingType.DisplayName.CreateXmlIds(landingName), null);
    //          if (createLandingId == null)
    //          {
    //            state.Operation.Message = "Посадочная страница с таким заголовком уже существует";
    //            return;
    //          }
    //          LightObject addLanding = new LightObject(box, createLandingId.Value);
    //          FabricHlp.SetCreateTime(addLanding);

    //          addLanding.Set(LandingType.FabricKind, ConvertHlp.ToInt(fabricKind));

    //          addLanding.Box.Update();

    //          state.CreatingObjectId = addLanding.Id;

    //          SiteContext.Default.UpdateStore();
    //        }
    //      ),
    //      DecorEdit.ReturnButton(UrlHlp.SeoUrl("landing-list", null))
    //    )
    //  ).EditContainer("editContent");
    //}

    //public static IHtmlControl GetLandingEdit(EditState state, int? landingId, out string title)
    //{
    //  title = "Редактирование посадочной страницы";

    //  if (landingId == null)
    //    landingId = state.CreatingObjectId;

    //  IShopStore shopStore = store as IShopStore;
    //  ShopStorage shop = shopStore.Shop;

    //  if (landingId == null)
    //    return GetLandingAdd(shop, state, out title);

    //  LightObject seo = shopStore.Landings.Find(landingId);

    //  if (seo == null)
    //    return EditHlp.GetInfoMessage("Неверный аргумент", "/");

    //  string returnUrl = UrlHlp.ShopUrl("landing", landingId);

    //  MetaKind kind = shop.FindFabricKind(seo.Get(LandingType.FabricKind));

    //  return new HPanel(
    //    DecorEdit.Title(seo.Get(FabricType.Identifier)),
    //    SeoPanelHlp.GetCommonPropertyPanel("landing", seo),
    //    new HPanel(
    //      DecorEdit.Field("Вид товара", new HLabel(MetaKindType.DisplayName(kind))),
    //      SeoPanelHlp.GetSortKindCombo(seo.Get(LandingType.SortKind)),
    //      SeoPanelHlp.GetEditFilterPanel(shopStore.SearchModule, seo, kind),
    //      DecorEdit.FieldArea("Поисковый запрос",
    //        new HTextEdit("searchText", seo.Get(LandingType.SearchText))
    //      )
    //    ).Margin(0, 10).MarginBottom(20),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.SaveButton()
    //      .Event("save_seo", "editContent",
    //        delegate (JsonData json)
    //        {
    //          string editTitle = (json.GetText("title") ?? "").Trim();
    //          string editDescription = (json.GetText("description") ?? "").Trim();
    //          string editText = (json.GetText("seoText") ?? "").Trim();
    //          string editSortKind = json.GetText("sortKind");

    //          string editHeading = (json.GetText("heading") ?? "").Trim();
    //          if (StringHlp.IsEmpty(editHeading))
    //          {
    //            state.Operation.Message = "Не задан заголовок страницы";
    //            return;
    //          }

    //          if (HttpContext.Current.IsInRole("nosave"))
    //          {
    //            state.Operation.Message = "Нет прав на сохранение изменений";
    //            return;
    //          }

    //          string editSearchText = (json.GetText("searchText") ?? "").Trim();
    //          //if (editSearchText.Length < 3)
    //          //{
    //          //  state.Operation.Message = "Поисковый запрос должен содержать не менее 3 символов";
    //          //  return;
    //          //}

    //          LightObject editSeo = DataBox.LoadObject(fabricConnection,
    //            LandingType.Landing, landingId.Value);

    //          if (!FabricType.Identifier.SetWithCheck(editSeo.Box, landingId.Value, editHeading))
    //          {
    //            state.Operation.Message = "Другая посадочная страница с таким заголовком уже существует";
    //            return;
    //          }

    //          editSeo.Set(ObjectType.ActTill, DateTime.UtcNow);

    //          editSeo.Set(SEOProp.Title, editTitle);
    //          editSeo.Set(SEOProp.Description, editDescription);
    //          editSeo.Set(SEOProp.Text, editText);

    //          editSeo.Set(LandingType.SearchText, editSearchText);
    //          editSeo.Set(LandingType.SortKind, editSortKind);

    //          if (kind != null)
    //          {
    //            SearchIndexStorage storage = shopStore.SearchModule.FindIndexStorage(kind.Id);
    //            if (storage != null)
    //            {
    //              SearchFilter filter = SearchHlp.GetFilterFromJson(storage, json);
    //              SearchHlp.FilterToLandingPage(storage, filter, editSeo);
    //            }
    //          }

    //          editSeo.Box.Update();

    //          SiteContext.Default.UpdateStore();
    //        }
    //      ),
    //      DecorEdit.ReturnButton(returnUrl)
    //    )
    //  ).EditContainer("editContent");
    //}


    //public static IHtmlControl GetLandingListEdit(EditState state, out string title)
    //{
    //  title = "Все посадочные страницы";

    //  IShopStore shopStore = store as IShopStore;

    //  List<IHtmlControl> elements = new List<IHtmlControl>();
    //  foreach (LightObject landing in shopStore.Landings.All)
    //  {
    //    HXPanel landingPanel = std.RowPanel(
    //      DecorEdit.RedoButton(true, "SEO", UrlHlp.SeoUrl("landing", landing.Id)).MarginTop(0),
    //      new HLink(UrlHlp.ShopUrl("landing", landing.Id),
    //        new HLabel(landing.Get(LandingType.DisplayName))
    //      ).PaddingLeft(10)
    //      //std.DockFill(),
    //      //std.Button("Удалить").Event("delete_landing", "",
    //      //  delegate (JsonData json)
    //      //  {
    //      //    if (HttpContext.Current.IsInRole("nosave"))
    //      //    {
    //      //      state.Operation.Message = "Нет прав на сохранение изменений";
    //      //      return;
    //      //    }

    //      //    SQLiteDatabaseHlp.DeleteObject(fabricConnection, landing.Id);
    //      //    Logger.AddMessage("Посадочная страница '{0}' успешно удалена", landing.Id);

    //      //    SiteContext.Default.UpdateStore();
    //      //  }
    //      //)
    //    ).VAlign(null).NoWrap().MarginBottom(5);

    //    elements.Add(landingPanel);
    //  }

    //  elements.Add(
    //    new HPanel(
    //      DecorEdit.RedoButton(true, "Добавить посадочную страницу", UrlHlp.SeoUrl("landing", null))
    //    ).MarginTop(10)
    //  );

    //  return new HPanel(
    //    DecorEdit.Title(title),
    //    new HPanel(
    //      elements.ToArray()
    //    ).Margin(0, 10).MarginBottom(20),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.ReturnButton("/")
    //    )
    //  );
    //}

    public static IHtmlControl GetSEOObjectEdit(HttpContext httpContext, WuiInitiator initiator, EditState state,
      string kind, int? id, out string title)
    {
      title = "Редактирование SEO полей страницы";

      //IShopStore shopStore = store as IShopStore;

      LightObject? seo = null;
      switch (kind)
      {
        case "page":
          seo = store.Sections.FindSection(id);
          break;
        //case "group":
        //  if (shopStore != null)
        //    seo = shopStore.Shop.FindGroup(id);
        //  break;
        //case "fabric":
        //  if (shopStore != null)
        //    seo = shopStore.Shop.FindFabric(id);
        //  break;
      }

      if (id == null || seo == null)
        return EditHlp.GetInfoMessage("Неверный аргумент", "/");

      string returnUrl = UrlHlp.ShopUrl(kind, id);

      return new HPanel(
        DecorEdit.Title(seo.Get(SectionType.Title).Name),
        new HPanel(
          DecorEdit.FieldArea("Титул страницы",
            new HTextEdit("title", seo.Get(SEOProp.Title))
          ),
          DecorEdit.FieldArea("H1 заголовок страницы",
            new HTextEdit("heading", seo.Get(SEOProp.Identifier).Name)
          ),
          DecorEdit.FieldArea("SEO описание",
            new HTextArea("description", seo.Get(SEOProp.Description))
          )
          //DecorEdit.FieldArea("SEO текст",
          //  new HTextArea("seoText", seo.Get(SEOProp.Text))
          //).Hide(kind != "fabric"),
          //DecorEdit.FieldBlock("SEO шаблоны для товаров",
          //  new HPanel(
          //    new HLabel("Вместо <<product>> подставляется наименование товара").MarginBottom(5),
          //    DecorEdit.FieldArea("Шаблон заголовка",
          //      new HTextEdit("productTitlePattern", seo.Get(SEOType.ProductTitlePattern))
          //    ),
          //    DecorEdit.FieldArea("Шаблон описания",
          //      new HTextArea("productDescriptionPattern", seo.Get(SEOType.ProductDescriptionPattern))
          //    )
          //  ).Padding(5, 8)
          //).Hide(kind != "group")
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .Event(initiator, "save_seo", "editContent",
            delegate (JsonData json)
            {
              string editTitle = (json.GetText("title") ?? "").Trim();
              string editDescription = (json.GetText("description") ?? "").Trim();
              //string editText = (json.GetText("seoText") ?? "").Trim();

              string editHeading = (json.GetText("heading") ?? "").Trim();
              if (StringHlp.IsEmpty(editHeading))
              {
                state.Operation.Warning("Не задан заголовок страницы");
                return;
              }

              if (httpContext.IsInRole("nosave"))
              {
                state.Operation.Warning("Нет прав на сохранение изменений");
                return;
              }

              using BoxDbContext dbContext = fabricConnection.Create();

              LightObject? editSeo = DataBox.LoadObject(fabricConnection, dbContext, id.Value);

              if (editSeo == null)
              {
                state.Operation.Error("Не найден объект для редактирования");
                return;
              }

              if (!SectionType.Title.SetWithCheck(dbContext, editSeo.Box, editSeo.Head, new NameId(editHeading)))
              {
                state.Operation.Warning("Другой объект с таким заголовком уже существует");
                return;
              }

              editSeo.Head.ActTo = DateTime.UtcNow;
              //editSeo.Set(ObjectType.ActTill, DateTime.UtcNow);

              editSeo.Set(SEOProp.Title, editTitle);
              editSeo.Set(SEOProp.Description, editDescription);
              //editSeo.Set(SEOProp.Text, editText);

              //if (kind == "group")
              //{
              //  string productTitlePattern = json.GetText("productTitlePattern");
              //  string productDescriptionPattern = json.GetText("productDescriptionPattern");
              //  editSeo.Set(SEOType.ProductTitlePattern, productTitlePattern);
              //  editSeo.Set(SEOType.ProductDescriptionPattern, productDescriptionPattern);
              //}

              editSeo.Box.Update(dbContext, true);

              BaseContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    public static IHtmlControl GetSEOPatternEdit(HttpContext httpContext, WuiInitiator initiator, EditState state, out string title)
    {
      bool isShop = false; // SiteContext.Default.Store is IShopStore;

      title = "Редактирование SEO полей";
      if (isShop)
        title += " и шаблонов";

      LightObject seo = store.SEO;

      string returnUrl = "/";

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.FieldBlock("SEO поля главной страницы",
            new HPanel(
              DecorEdit.FieldArea("Титул",
                new HTextEdit("mainTitle", seo.Get(SEOType.MainTitle))
              ),
              DecorEdit.FieldArea("Описание",
                new HTextArea("mainDescription", seo.Get(SEOType.MainDescription))
              )
              //DecorEdit.FieldArea("Ключевые слова",
              //  new HTextEdit("mainKeywords", seo.Get(SEOType.MainKeywords))
              //)
            ).Padding(5, 8)
          ).MarginBottom(20),
          DecorEdit.FieldBlock("SEO поля для всех страниц",
            new HPanel(
              DecorEdit.FieldArea("SEO текст в подвале",
                new HTextArea("footerSeoText", seo.Get(SEOType.FooterSeoText))
              )
            ).Padding(5, 8)
          ).MarginBottom(20),
          DecorEdit.FieldBlock("SEO шаблоны для страниц",
            new HPanel(
              new HLabel("Вместо <<title>> подставляется заголовок страницы").MarginBottom(5),
              DecorEdit.FieldArea("Шаблон заголовка",
                new HTextEdit("sectionTitlePattern", seo.Get(SEOType.SectionTitlePattern))
              ),
              DecorEdit.FieldArea("Шаблон описания",
                new HTextArea("sectionDescriptionPattern", seo.Get(SEOType.SectionDescriptionPattern))
              )
            ).Padding(5, 8)
          ).MarginBottom(20)
          //DecorEdit.FieldBlock("SEO шаблоны для групп",
          //  new HPanel(
          //    new HLabel("Вместо <<group>> подставляется наименование группы").MarginBottom(5),
          //    DecorEdit.FieldArea("Шаблон заголовка",
          //      new HTextEdit("groupTitlePattern", seo.Get(SEOType.GroupTitlePattern))
          //    ),
          //    DecorEdit.FieldArea("Шаблон описания",
          //      new HTextArea("groupDescriptionPattern", seo.Get(SEOType.GroupDescriptionPattern))
          //    )
          //  ).Padding(5, 8)
          //).Hide(!isShop).MarginBottom(20),
          //DecorEdit.FieldBlock("SEO шаблоны для товаров",
          //  new HPanel(
          //    new HLabel("Вместо <<product>> подставляется наименование товара").MarginBottom(5),
          //    DecorEdit.FieldArea("Шаблон заголовка",
          //      new HTextEdit("productTitlePattern", seo.Get(SEOType.ProductTitlePattern))
          //    ),
          //    DecorEdit.FieldArea("Шаблон описания",
          //      new HTextArea("productDescriptionPattern", seo.Get(SEOType.ProductDescriptionPattern))
          //    )
          //  ).Padding(5, 8)
          //).Hide(!isShop)
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .Event(initiator, "save_seo", "editContent",
            delegate (JsonData json)
            {
              if (httpContext.IsInRole("nosave"))
              {
                state.Operation.Warning("Нет прав на сохранение изменений");
                return;
              }

              string mainTitle = json.GetText("mainTitle");
              string mainDescription = json.GetText("mainDescription");
              string mainKeywords = json.GetText("mainKeywords");
              string footerSeoText = json.GetText("footerSeoText");
              string sectionTitlePattern = json.GetText("sectionTitlePattern");
              string sectionDescriptionPattern = json.GetText("sectionDescriptionPattern");
              //string groupTitlePattern = json.GetText("groupTitlePattern");
              //string groupDescriptionPattern = json.GetText("groupDescriptionPattern");
              //string productTitlePattern = json.GetText("productTitlePattern");
              //string productDescriptionPattern = json.GetText("productDescriptionPattern");

              using BoxDbContext dbContext = fabricConnection.Create();

              LightObject? editSeo = DataBox.LoadObject(fabricConnection, dbContext,
                SEOType.SEO, ContactsType.Kind.Create(new KindId("main")));

              if (editSeo == null)
              {
                state.Operation.Error("Не найден объект для редактирования");
                return;
              }

              editSeo.Set(SEOType.MainTitle, mainTitle);
              editSeo.Set(SEOType.MainDescription, mainDescription);
              editSeo.Set(SEOType.FooterSeoText, footerSeoText);
              editSeo.Set(SEOType.SectionTitlePattern, sectionTitlePattern);
              editSeo.Set(SEOType.SectionDescriptionPattern, sectionDescriptionPattern);
              //editSeo.Set(SEOType.GroupTitlePattern, groupTitlePattern);
              //editSeo.Set(SEOType.GroupDescriptionPattern, groupDescriptionPattern);
              //editSeo.Set(SEOType.ProductTitlePattern, productTitlePattern);
              //editSeo.Set(SEOType.ProductDescriptionPattern, productDescriptionPattern);

              editSeo.Box.Update(dbContext, true);

              BaseContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }
  }
}