﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;
using Commune.Data;
using Commune.Html;
using NitroBolt.Wui;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace Site.Engine
{
  public class SorterHlp
  {
    static DataLayer fabricConnection
    {
      get
      {
        return BaseContext.Default.FabricConnection;
      }
    }

    public static IHtmlControl GetSortingEdit(HttpContext httpContext, WuiInitiator initiator, EditState state,
      string title, string sortKind, LightObject[] items, string returnUrl)
    {
      if (sortKind == "desc")
        return GetTimeDescSortingEdit(httpContext, initiator, state, title, items, returnUrl);

      return GetAlhabetSortingEdit(httpContext, initiator, state, title, items, returnUrl);
    }

    public static IHtmlControl GetAlhabetSortingEdit(HttpContext httpContext, WuiInitiator initiator, EditState state,
      string title, LightObject[] items, string returnUrl)
    {
      int[] itemIds = ArrayHlp.Convert(items,
        delegate (LightObject section) { return section.Id; }
      );

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          new HTextView("Если нужно, чтобы раздел показывался <em>перед</em> другими разделами, то задайте префикс начинающийся с <strong>aa</strong>."
          ).Padding(5),
          new HTextView("Если нужно, чтобы раздел показывался <em>после</em> других разделов, то задайте префикс начинающийся с <strong>Яя</strong>."
          ).Padding(5),
          new HPanel(
            items.Select((item, index) =>
            {
              HPanel row = new HPanel(
                new HTextEdit(string.Format("prefix_{0}", item.Id), item.Get(SEOProp.SortingPrefix))
                  .Width(100).Margin(0, 5),
                new HLabel(GetDisplayName(item))
              ).Padding(5);

              if (index % 2 == 0)
                row.Background("#F9F9F9");

              return row;
            }).ToArray()
          ).MarginTop(10)
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .Event(initiator, "save_sorting", "editContent",
            delegate (JsonData json)
            {
              if (httpContext.IsInRole("nosave"))
              {
                state.Operation.Warning("Нет прав на сохранение изменений");
                return;
              }

              using BoxDbContext dbContext = fabricConnection.Create();

              ObjectBox editBox = new(fabricConnection, dbContext, db => db.Objects.ForObjects(itemIds));

              foreach (int editId in editBox.AllObjectIds)
              {
                string prefix = json.GetText(string.Format("prefix_{0}", editId));
                if (prefix == null)
                  continue;
                LightObject editSection = new LightObject(editBox, editId);
                editSection.Set(SEOProp.SortingPrefix, prefix);
              }

              editBox.Update(dbContext, true);

              BaseContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    static IHtmlControl GetTimeDescSortingEdit(HttpContext httpContext, WuiInitiator initiator, EditState state,
      string title, LightObject[] items, string returnUrl)
    {
      int[] itemIds = ArrayHlp.Convert(items,
        delegate (LightObject item) { return item.Id; }
      );

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
					new HPanel(
						items.Select((item, index) =>
						{
							HPanel row = new HPanel(
								new HTextEdit(string.Format("sortTime_{0}", item.Id), TimeToString(item.Get(SEOProp.SortTime)))
									.Width(140).Margin(0, 5).Placeholder(TimeToString(item.Head.ActFrom)),
								new HLabel(GetDisplayName(item))
							).Padding(5);

							if (index % 2 == 0)
								row.Background("#F9F9F9");

							return row;
						}).ToArray()
					).MarginTop(10)
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .Event(initiator, "save_sorting", "editContent",
            delegate (JsonData json)
            {
              if (httpContext.IsInRole("nosave"))
              {
                state.Operation.Warning("Нет прав на сохранение изменений");
                return;
              }

              using BoxDbContext dbContext = fabricConnection.Create();

              ObjectBox editBox = new(fabricConnection, dbContext, db => db.Objects.ForObjects(itemIds));

              foreach (int editId in editBox.AllObjectIds)
              {
                string rawSortTime = json.GetText(string.Format("sortTime_{0}", editId));
                LightObject editItem = new(editBox, editId);
                ParseAndSetSortTime(editItem, rawSortTime);
              }

              editBox.Update(dbContext, true);

              BaseContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    public static HComboEdit<string> SortKindCombo(string dataName, string selected)
    {
      return new HComboEdit<string>(dataName, selected,
        delegate (string kind)
        {
          if (kind == "desc")
            return "По убыванию даты";
          return "По алфавиту";
        },
        new string[] { "", "desc" }
      );
    }

    public const string sortTimeFormat = "dd.MM.yyyy HH:mm";
    public static string TimeToString(DateTime? time)
    {
      return time?.ToLocalTime().ToString(sortTimeFormat) ?? "";
    }

    public static IHtmlControl SortTimeEdit(LightKin edit, bool hide)
    {
      return DecorEdit.Field("Дата для сортировки", "sortTime",
        TimeToString(edit.Get(SEOProp.SortTime))
      ).MarginLeft(5).Hide(hide)
        .Placeholder(TimeToString(edit.Head.ActFrom));
    }

    public static bool ParseAndSetSortTime(LightObject edit, string rawSortTime)
    {
      if (StringHlp.IsEmpty(rawSortTime))
      {
        edit.Set(SEOProp.SortTime, null);
        return true;
      }

      DateTime sortTime;
      if (DateTime.TryParse(rawSortTime, out sortTime))
      {
        edit.Set(SEOProp.SortTime, sortTime.ToUniversalTime());
        return true;
      }

      return false;
    }

    public static string GetDisplayName(LightObject item)
    {
      if (item is LightSection)
        return ((LightSection)item).NameInMenu;

			return SEOProp.GetDisplayName(item);
      //return item.Get(UnitType.DisplayName);
    }

    public static T[] Sort<T>(List<T> items, string sortKind) where T : LightObject
    {
      T[] itemArray = items.ToArray();

      if (sortKind == "desc")
      {
        ArrayHlp.Sort(itemArray, delegate (LightObject item)
          {
            DateTime sortTime = item.Get(SEOProp.SortTime) ??
              item.Head.ActFrom ?? DateTime.MinValue;
            return -sortTime.Ticks;
          }
        );
      }
      else
      {
        ArrayHlp.Sort(itemArray, delegate (LightObject item)
        {
          return item.Get(SEOProp.SortingPrefix) + GetDisplayName(item);
        });
      }

      return itemArray;
    }
    
  }
}
