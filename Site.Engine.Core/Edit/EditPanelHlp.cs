using System;
using System.Collections.Generic;
using System.Linq;
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
  public class EditPanelHlp
  {
    static DataLayer fabricConnection
    {
      get
      {
        return BaseContext.Default.FabricConnection;
      }
    }

    //public static IHtmlControl GetFabricParentsPanel(EditState state, ShopStorage shop, LightKin fabric)
    //{
    //  RowLink[] parentRows = fabric.AllParentRows(GroupType.FabricTypeLink);

    //  int[] groupIdsForAdd = FabricHlp.FindGroupIdsForAddFabric(shop, fabric);

    //  List<IHtmlControl> controls = new List<IHtmlControl>();

    //  controls.Add(
    //    DecorEdit.FieldBlock("Товар находится в группах:",
    //      new HGrid<RowLink>(parentRows, delegate (RowLink parentRow)
    //        {
    //          int linkId = parentRow.Get(LinkType.LinkId);
    //          int parentId = parentRow.Get(LinkType.ParentId);
    //          LightGroup parentGroup = shop.FindGroup(parentId);

    //          return new HPanel(
    //            new HLabel(parentGroup?.Get(GroupType.Identifier)).Width(300).Padding(10, 0),
    //            parentRows.Length < 2 ? null :
    //              std.Button("Убрать из группы").MarginLeft(20)
    //                .Event("remove_from_group", "",
    //                delegate
    //                {
    //                  fabricConnection.GetScalar("", "Delete From light_link Where link_id = @linkId",
    //                    new DbParameter("linkId", linkId)
    //                  );
    //                  SiteContext.Default.UpdateStore();
    //                },
    //                linkId
    //              )
    //          );
    //        },
    //        new HRowStyle()
    //      ).Padding(0, 10)
    //    ).InlineBlock()
    //  );

    //  controls.Add(
    //    new HPanel(
    //      new HComboEdit<int>("group_for_add", -1,
    //        delegate (int groupId) { return shop.FindGroup(groupId)?.Get(GroupType.Identifier); },
    //        groupIdsForAdd
    //      ).MarginRight(20),
    //      std.Button("Добавить в группу").Event("add_in_group", "group_add_container",
    //        delegate (JsonData json)
    //        {
    //          int? addGroupId = ConvertHlp.ToInt(json.GetText("group_for_add"));
    //          if (addGroupId == null || addGroupId == -1)
    //          {
    //            state.Operation.Message = "Группа для добавления не выбрана";
    //            return;
    //          }

    //          LightKin editFabric = DataBox.LoadKin(fabricConnection, FabricType.Fabric, fabric.Id);
    //          editFabric.AddParentId(GroupType.FabricTypeLink, addGroupId.Value);
    //          editFabric.Box.Update();

    //          SiteContext.Default.UpdateStore();
    //        }
    //      )
    //    ).EditContainer("group_add_container")
    //  );

    //  controls.Add(
    //    EditElementHlp.GetDeletePanel(state, fabric.Id, "товар", "Удаление товара", null)
    //  );

    //  return new HPanel(controls.ToArray());
    //}

    //public static IHtmlControl GetObjectAdd(EditState state,
    //  string title, string fieldCaption, string returnUrl,
    //  int objectType, XmlDisplayName displayName)
    //{
    //  return GetObjectAdd(state, title, fieldCaption, returnUrl,
    //    delegate (string objectName)
    //    {
    //      ObjectBox box = new ObjectBox(fabricConnection, "1=0");
    //      int? createObjectId = box.CreateUniqueObject(objectType,
    //        displayName.CreateXmlIds(objectName), null);

    //      if (createObjectId == null)
    //      {
    //        state.Operation.Message = "Объект с таким наименованием уже существует";
    //        return null;
    //      }

    //      return new LightObject(box, createObjectId.Value);
    //    }
    //  );
    //}

    //public static IHtmlControl GetObjectAdd(HttpContext httpContext, EditState state,
    //  string title, string fieldCaption, string returnUrl,
    //  Func<string, LightObject> objectCreator)
    //{
    //  return new HPanel(
    //    DecorEdit.Title(title),
    //    new HPanel(
    //      DecorEdit.Field(fieldCaption, "objectName", "")
    //    ).Margin(0, 10).MarginBottom(20),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.AddButton("Добавить").Event("add_object", "addContent",
    //        delegate (JsonData json)
    //        {
    //          string objectName = json.GetText("objectName");
    //          if (StringHlp.IsEmpty(objectName))
    //          {
    //            state.Operation.Message = "Не задано наименование добавляемого объекта";
    //            return;
    //          }

    //          if (httpContext.IsInRole("nosave"))
    //          {
    //            state.Operation.Message = "Нет прав на сохранение изменений";
    //            return;
    //          }

    //          LightObject createObject = objectCreator(objectName);
    //          if (createObject == null)
    //            return;

    //          FabricHlp.SetCreateTime(createObject);
    //          createObject.Box.Update();

    //          state.CreatingObjectId = createObject.Id;

    //          SiteContext.Default.UpdateStore();
    //        }
    //      ),
    //      DecorEdit.ReturnButton(returnUrl)
    //    )
    //  ).EditContainer("addContent");
    //}
  }
}
