using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NitroBolt.Wui;
using Commune.Html;
using Commune.Data;
using Commune.Basis;
using System.IO;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Site.Engine
{
  public class UnitEditorHlp
  {
    static DataLayer fabricConnection
    {
      get
      {
        return BaseContext.Default.FabricConnection;
      }
    }

    static IStore store
    {
      get { return BaseContext.Default.Store; }
    }

    public static void CheckAndCreatePane(LightSection section, int paneIndex, string designKind)
    {
      if (section.UnitForPane(paneIndex) != null)
				return;

			using BoxDbContext dbContext = fabricConnection.Create();

      LightParent? editSection = DataBox.LoadParent(fabricConnection, dbContext, section.Id);
      if (editSection == null)
        return;

			ObjectBox box = new(fabricConnection, dbContext);
			int createUnitId = box.CreateAndAddObject(UnitType.Unit, UnitType.JsonId.Create(new ParentNameId(section.Id, "")), null);

      LightObject addUnit = new(box, createUnitId);
      addUnit.SetCreateTime();

      addUnit.Set(SectionType.DesignKind, designKind);

      editSection.SetChildId(SectionType.UnitForPaneLinks, paneIndex, createUnitId);
      //addUnit.SetParentId(SectionType.UnitForPaneLinks, paneIndex, section.Id);

      addUnit.Box.Update(dbContext, false);
      editSection.Box.Update(dbContext, false);

      dbContext.SaveChanges();

      BaseContext.Default.UpdateStore();
    }

    public static IHtmlControl GetUnitAdd(HttpContext httpContext, EditorSelector selector,
      WuiInitiator initiator, EditState state, string title, LightKin parent, string fixedDesignKind)
    {
      LightSection? parentSection = parent is LightSection ? (LightSection)parent :
        FabricHlp.ParentSectionForUnit(store.Sections, parent);
      string returnUrl = UrlHlp.ReturnUnitUrl(parentSection, parent.Id);
      //string returnUrl = parent.IsMenu ? "/" : UrlHlp.ShopUrl("page", parent.Id);

      string[] allKinds = selector.AllKinds;
      if (!StringHlp.IsEmpty(fixedDesignKind) && allKinds.Contains(fixedDesignKind))
        allKinds = new string[] { fixedDesignKind };

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field(
            new HLabel("Тип элемента").FontBold(),
            new HComboEdit<string>("designKind", "",
              delegate (string kind)
              {
                return selector.GetDisplayName(kind);
              },
              allKinds
            )
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить").Event(initiator, "add_unit", "addContent",
            delegate (JsonData json)
            {
              string unitName = json.GetText("name");
              if (StringHlp.IsEmpty(unitName))
              {
                state.Operation.Warning("Не задан заголовок элемента");
                return;
              }

              string designKind = json.GetText("designKind");

              using BoxDbContext dbContext = fabricConnection.Create();

              LightParent? editParent = DataBox.LoadParent(fabricConnection, dbContext, parent.Id);

              if (editParent == null)
              {
                state.Operation.Error("Не найден родительский элемент");
                return;
              }

              ObjectBox box = new(fabricConnection, dbContext);
              int? createUnitId = box.CreateUniqueObject(dbContext, UnitType.Unit,
                UnitType.JsonId.Create(new ParentNameId(parent.Id, unitName)), null
              );

              if (createUnitId == null)
              {
                state.Operation.Warning("Элемент с таким заголовком уже существует");
                return;
              }

              if (httpContext.IsInRole("nosave"))
              {
                state.Operation.Warning("Нет прав на сохранение изменений");
                return;
              }

              LightObject addUnit = new(box, createUnitId.Value);
              addUnit.SetCreateTime();

              addUnit.Set(SectionType.DesignKind, designKind);

              editParent.AddChildLink(UnitType.SubunitLinks, addUnit.Id);
              //addUnit.AddParentId(UnitType.SubunitLinks, parent.Id);

              addUnit.Box.Update(dbContext, false);
              editParent.Box.Update(dbContext, false);

              dbContext.SaveChanges();

              state.CreatingObjectId = addUnit.Id;

              BaseContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("addContent");
    }

    public static IHtmlControl GetEditor(HttpContext httpContext, WuiInitiator initiator, EditState state, LightKin unit, BaseTunes tunes)
    {
      bool hideTile = !tunes.GetTune("Tile");
      bool hideSortTime = !tunes.GetTune("SortTime");
			bool hideAdaptTitle = !tunes.GetTune("AdaptTitle");
			bool hideAdaptImage = !tunes.GetTune("AdaptImage");
      bool hideImageAlt = !tunes.GetTune("ImageAlt");
      bool hideTag1 = !tunes.GetTune("Tag1");
      bool hideTag2 = !tunes.GetTune("Tag2");
      bool hideSubtitle = !tunes.GetTune("Subtitle");
      bool hideAnnotation = !tunes.GetTune("Annotation");
      bool hideContent = !tunes.GetTune("Content");
      bool hideLink = !tunes.GetTune("Link");
      bool hideGallery = !tunes.GetTune("Gallery");
      bool hideSubunits = !tunes.GetTune("Subunits");
      bool hideSortKind = !tunes.GetTune("SortKind");

      //Log.Information("GetEditor: {0}, {1}, {2}", tunes.DesignKind, hideAnnotation, hideContent);

			LightSection? parentSection = FabricHlp.ParentSectionForUnit(store.Sections, unit);
      string returnUrl = UrlHlp.ReturnUnitUrl(parentSection, unit.Id);
      //Logger.AddMessage("Parent: {0}, {1}, {2}", unit.Id, parentSection != null, returnUrl);

      return new HPanel(
        DecorEdit.Title("Редактирование элемента страницы"),
        GetDeletePanel(httpContext, initiator, state, unit),
        new HPanel(
          DecorEdit.Field("Заголовок элемента", "title", unit.Get(UnitType.JsonId).Name)
            .MarginLeft(5),
					DecorEdit.Field("Адаптивный заголовок", "adaptTitle", unit.Get(UnitType.AdaptTitle))
						.MarginLeft(5).Hide(hideAdaptTitle),
					new HPanel(
						EditElementHlp.GetImageThumb(initiator, unit.Id, tunes).InlineBlock().MarginRight(20)
							.Hide(hideTile),
						EditElementHlp.GetAdaptImage(initiator, unit.Id).InlineBlock()
							.Hide(hideAdaptImage)
					).MarginLeft(5),
          DecorEdit.FieldArea("Альтернативный текст для картинки",
            new HTextArea("imageAlt", unit.Get(UnitType.ImageAlt)).Height("2em")
          ).Hide(hideImageAlt).MarginLeft(5),
          DecorEdit.Field(
            new HLabel("Cортировка элементов").FontBold(),
            SorterHlp.SortKindCombo("sortKind", unit.Get(UnitType.SortKind))
          ).MarginLeft(5).Hide(hideSortKind),
          SorterHlp.SortTimeEdit(unit, hideSortTime),
          DecorEdit.Field("Подзаголовок", "subtitle", unit.Get(UnitType.Subtitle))
            .Hide(hideSubtitle).MarginLeft(5),
          DecorEdit.FieldArea("Аннотация",
            new HTextArea("annotation", unit.Get(UnitType.Annotation)).Height("8em")
          ).Hide(hideAnnotation).MarginLeft(5),
          DecorEdit.Field("Адрес ссылки", "link", unit.Get(UnitType.Link))
            .Hide(hideLink).MarginLeft(5),
          DecorEdit.Field("Признак 1", "tag1", unit.Get(UnitType.Tags, 0))
            .Hide(hideTag1).MarginLeft(5),
          DecorEdit.Field("Признак 2", "tag2", unit.Get(UnitType.Tags, 1))
            .Hide(hideTag2).MarginLeft(5),
          new HPanel(
            DecorEdit.FieldInputBlock("Текст",
              HtmlHlp.CKEditorCreate("content", unit.Get(UnitType.Content),
                "400px", true)
            ),
            EditElementHlp.GetDescriptionImagesPanel(initiator, state, unit.Id)
          ).Hide(hideContent),
          GalleryEditorHlp.GetGalleryPanel(initiator, state, unit.Id, tunes)
            .Hide(hideGallery)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event(initiator, "save_unit", "editContent",
            delegate (JsonData json)
            {
              string title = json.GetText("title");
              if (StringHlp.IsEmpty(title))
              {
                state.Operation.Warning("Не задан заголовок");
                return;
              }

							string adaptTitle = json.GetText("adaptTitle");
              string imageAlt = json.GetText("imageAlt");
              string sortKind = json.GetText("sortKind");
              string subtitle = json.GetText("subtitle");
              string annotation = json.GetText("annotation");
              string link = json.GetText("link");
              string tag1 = json.GetText("tag1");
              string tag2 = json.GetText("tag2");
              string content = json.GetText("content");

              if (httpContext.IsInRole("nosave"))
              {
                state.Operation.Warning("Нет прав на сохранение изменений");
                return;
              }

              using BoxDbContext dbContext = fabricConnection.Create();

              LightObject? editUnit = DataBox.LoadObject(fabricConnection, dbContext, unit.Id);
              if (editUnit == null)
              {
                state.Operation.Warning("Элемент не найден");
                return;
              }

              if (!UnitType.JsonId.SetWithCheck(dbContext, editUnit.Box, editUnit.Head, new ParentNameId(parentSection?.Id ?? -1, title)))
              {
                state.Operation.Warning("Элемент с таким заголовком уже существует");
                return;
              }
              
							if (!hideAdaptTitle)
								editUnit.Set(UnitType.AdaptTitle, adaptTitle);
              if (!hideImageAlt)
                editUnit.Set(UnitType.ImageAlt, imageAlt);
              if (!hideSortKind)
                editUnit.Set(UnitType.SortKind, sortKind);
              if (!hideSortTime)
                SorterHlp.ParseAndSetSortTime(editUnit, json.GetText("sortTime"));
              if (!hideSubtitle)
                editUnit.Set(UnitType.Subtitle, subtitle);
              if (!hideAnnotation)
                editUnit.Set(UnitType.Annotation, annotation);
              if (!hideLink)
                editUnit.Set(UnitType.Link, link);
              if (!hideTag1)
                editUnit.Set(UnitType.Tags, 0, tag1);
              if (!hideTag2)
                editUnit.Set(UnitType.Tags, 1, tag2);
              if (!hideContent)
                editUnit.Set(UnitType.Content, content);

              editUnit.Box.Update(dbContext, true);

              BaseContext.Default.UpdateStore();

              state.Operation.Success("Изменения успешно сохранены");
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    static IHtmlControl GetDeletePanel(HttpContext httpContext, WuiInitiator initiator, EditState state, LightKin unit)
    {
      return EditElementHlp.GetDeletePanel(httpContext, initiator, state, unit.Id, "", "Удаление элемента страницы",
        delegate
        {
          return true;
        }
      );
    }
  }
}
