﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using NitroBolt.Wui;

namespace Commune.Html
{
  public class HFileUploader : ExtensionContainer, IHtmlControl
  {
    readonly string fileUploadJsPath;
    readonly string caption;
    readonly object objectId;
		readonly int sizeLimit;
		readonly string onComplete;
    readonly HAttribute[] attributes;

    /// <summary>
    /// Для использования подключите fileuploader.css и fileuploader.js.
    /// Реализуйте FileUploader : IHttpHandler
    /// Пропишите в web.config:
    /// <add name="fileUploader" verb="*" path="<<fileUploadJsPath>>" type="<<YourNamespace.FileUploader>>"/>
    /// </summary>
    /// <param name="fileUploadJsPath">Например, fileupload.js</param>
    /// <param name="objectId">Идентификатор объекта, к которому относятся загружаемые файлы</param>
    public HFileUploader(string fileUploadJsPath, string caption, object objectId,
			int sizeLimit, string onComplete, params HAttribute[] attributes) :
      base("HFileUploader", "")
    {
      this.fileUploadJsPath = fileUploadJsPath;
      this.caption = caption;
      this.objectId = objectId;
			this.sizeLimit = sizeLimit;
			this.onComplete = onComplete;
      this.attributes = attributes;
    }

		public HFileUploader(string fileUploadJsPath, string caption, object objectId, 
			params HAttribute[] attributes) :
			this(fileUploadJsPath, caption, objectId, 0, null, attributes)
		{
		}


		static readonly HBuilder h = HBuilder.Extension;

    public HElement ToHtml(string cssClassName, StringBuilder css)
    {
      List<object> content = new List<object>();
      {
        StringBuilder builder = new StringBuilder();
        builder.Append("new qq.FileUploader({element: this");
        builder.AppendFormat(", action: '{0}'", fileUploadJsPath);
        builder.Append(", encoding: 'multipart'");
        builder.AppendFormat(", uploadButtonText: '{0}'", caption);
				if (sizeLimit != 0)
				{
					builder.AppendFormat(", sizeLimit: {0}", sizeLimit);
				}
				if (!StringHlp.IsEmpty(onComplete))
				{
					builder.AppendFormat(", onComplete: function (data) {{ {0} }}", onComplete);
				}
				builder.Append(", params: {");
        builder.AppendFormat("objectId: '{0}'", objectId);
        foreach (HAttribute attr in attributes)
        {
          builder.AppendFormat(", {0}: '{1}'", attr.Name, attr.Value);
        }
        builder.Append("}})");

        content.Add(h.Attribute("js-init", builder.ToString())
        );
      }
      foreach (TagExtensionAttribute extension in TagExtensions)
        content.Add(new HAttribute(extension.Name, extension.Value));

      return h.Div(content.ToArray());
    }
  }
}
