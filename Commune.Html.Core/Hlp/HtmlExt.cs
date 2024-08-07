﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroBolt.Wui;
using Commune.Basis;
using Microsoft.AspNetCore.Components.Web;

namespace Commune.Html
{
  public static class HtmlExt
  {
    public static object GetData(this JsonData json, string dataName)
    {
      return json.JPath("data", dataName);
    }

    public static string GetText(this JsonData json, string dataName)
    {
      object data = json.GetData(dataName);
      if (data == null)
        return "";
      return data.ToString() ?? "";
    }

    public static bool GetBool(this JsonData json, string dataName)
    {
      string? rawData = json.GetText(dataName);
      return rawData?.ToLower() == "true";
    }

    public static int? GetInt(this JsonData json, string dataName)
    {
      return ConvertHlp.ToInt(json.GetData(dataName));
    }

    public static T Media<T>(this T control, string queryWithBrackets, params HStyle[] styles)
      where T : IEditExtension
    {
      control.WithExtension(new MediaExtensionAttribute(queryWithBrackets, styles));
      return control;
    }

    public static T Media<T>(this T control, int maxWidth, params HStyle[] styles)
      where T : IEditExtension
    {
			return control.Media(string.Format("(max-width: {0}px)", maxWidth - 1), styles);
			//return control.Media(string.Format("(max-device-width: {0}px)", maxWidth), styles);
		}

    public static T Hide<T>(this T control, bool hide) where T : IEditExtension
    {
      control.WithExtension(new ExtensionAttribute("hide", hide));
      return control;
    }

    public static T Value<T>(this T control, object value) where T : IEditExtension
    {
      control.WithExtension(new ExtensionAttribute("value", value));
      return control;
    }

    public static T EditContainer<T>(this T control, string container) where T : IEditExtension
    {
      control.WithExtension(new ExtensionAttribute("container", container));
      return control;
    }

    //public static T Event<T>(this T control, hevent onevent) where T : IEventEditExtension
    //{
    //  control.WithExtension(new ExtensionAttribute("onevent", onevent));
    //  return control;
    //}

    public static T Event<T>(this T control, WuiInitiator initiator, string command, string editContainer, 
      Action<JsonData> eventHandler, params object[] extraIds) where T : IEventEditExtension
    {
      hdata onevent = InnerEvent(command, editContainer, extraIds);

      if (initiator.CallKind == WuiCallKind.Json && initiator.Json != null && initiator.FoundEvent == null)
      {
        if (IsFoundEvent(onevent, initiator.Json))
					initiator.FoundEvent = eventHandler;
      }

			control.WithExtension(new ExtensionAttribute("onevent", onevent));
			return control;
    }

    static bool IsFoundEvent(hdata onevent, JsonData json)
    {
			foreach (HAttribute id in onevent)
			{
				object jsonId = json.JPath(id.Name.LocalName.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries));
        if (jsonId == null)
          return false;

        if (StringHlp.ToString(id.Value) != StringHlp.ToString(jsonId))
          return false;
			}
      return true;
		}

		static hdata InnerEvent(string command, string editContainer, params object[] extraIds)
    {
      hdata onevent = new() { { "command", command } };

      if (!StringHlp.IsEmpty(editContainer))
        onevent.Add("container", editContainer);

      int i = -1;
      foreach (object id in extraIds)
      {
        ++i;
        onevent.Add(string.Format("id{0}", i + 1), id);
      }

      return onevent;
    }

    //public static hevent InnerEvent(string command, string editContainer,
    //  Action<JsonData> eventHandler, params object[] extraIds)
    //{
    //  hevent onevent = new hevent(delegate (object[] ids, JsonData json)
    //  {
    //    eventHandler(json);
    //  }) { { "command", command } };

    //  if (!StringHlp.IsEmpty(editContainer))
    //    onevent.Add("container", editContainer);

    //  int i = -1;
    //  foreach (object id in extraIds)
    //  {
    //    ++i;
    //    onevent.Add(string.Format("id{0}", i + 1), id);
    //  }

    //  return onevent;
    //}

    public static T ExtraClassNames<T>(this T control, params string[] classNames) where T : IEditExtension
    {
      control.WithExtension(new ExtensionAttribute("extraClassNames", classNames));
      return control;
    }
    
    /// <summary>
    /// Поддерживается элементами выпадающего списка DropDown.
    /// Клик на невыбираемый элемент не закрывает список. Невыбираемые элементы не используют стили AnyItem.
    /// </summary>
    public static T Unselectable<T>(this T control) where T : IEditExtension
    {
      control.WithExtension(new ExtensionAttribute("unselectable", true));
      return control;
    }

    public static bool Validate(this WebOperation operation, bool isError, string errorMessage)
    {
      if (isError)
      {
        operation.Warning(errorMessage);
        return false;
      }

      return true;
    }

    public static bool Validate(this WebOperation operation, string value, string errorMessage)
    {
      return Validate(operation, StringHlp.IsEmpty(value), errorMessage);
    }
  }

  public class WebOperation
  {
    DialogIcon? status = null;
    public DialogIcon? Status
    {
      get { return status; }
    }
    string message = "";
    public string Message
    {
      get { return message; }
    }
    bool completed = false;
    public bool Completed
    {
      get { return completed; }
    }
    public string ReturnUrl = "";

		public volatile int Counter = 0;
		public string Actual(string command)
		{
			return string.Format("{0}_{1}", command, Counter);
		}

		public void Reset()
    {
      status = null;
      message = "";
      completed = false;
      ReturnUrl = "";
    }

    public bool IsSuccess
    {
      get { return status == DialogIcon.Success; }
    }
    public bool IsWarning
    {
      get { return status == DialogIcon.Warning; }
    }
    public bool IsError
    {
      get { return status == DialogIcon.Error; }
    }

    public void Success(string message)
    {
      this.status = DialogIcon.Success;
      this.message = message;
    }

    public void Warning(string message)
    {
      this.status = DialogIcon.Warning;
      this.message = message;
    }

    public void Error(string message)
    {
      this.status = DialogIcon.Error;
      this.message = message;
    }

    public void Dialog(DialogIcon status, string message)
    {
      this.status = status;
      this.message = message;
    }

    public void Complete(string message, string returnUrl)
    {
      this.completed = true;
      this.message = message;
      this.ReturnUrl = returnUrl;
    }

    public WebOperation()
    {
    }
  }

  public enum DialogIcon
  {
    Info,
    Warning,
    Question,
    Error,
    Success
  }

  //public class DialogIcon
  //{
  //  public const string Info = "Info";
  //  public const string Warning = "Warning";
  //  public const string Question = "Question";
  //  public const string Error = "Error";
  //  public const string Success = "Success";
  //}
}
