using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using NitroBolt.Wui;

namespace Commune.Html
{
  public class HComboEdit<T> : ExtensionContainer, IHtmlControl
    where T : notnull
  {
    public HComboEdit(string dataName, T selected, Func<T, string> displayGetter, params T[] comboItems) :
      base("HComboEdit", dataName)
    {
      this.selected = selected;
      this.comboItems = ArrayHlp.Convert(comboItems, delegate (T comboItem)
        {
          string value = displayGetter(comboItem);
          return new Tuple<T, string>(comboItem, value);
        }
      );
    }

    readonly T selected;
    readonly Tuple<T, string>[] comboItems;

    public HComboEdit(string dataName, T selected, params Tuple<T, string>[] comboItems) :
      base("HComboEdit", dataName)
    {
      this.selected = selected;
      this.comboItems = comboItems;
    }

    static readonly HBuilder h = HBuilder.Extension;

    public HElement ToHtml(string cssClassName, StringBuilder css)
    {
      DefaultExtensionContainer defaults = new DefaultExtensionContainer(this);
      defaults.Cursor(CursorStyle.Pointer);

      HtmlHlp.AddClassToCss(css, cssClassName, CssExtensions);

			HtmlHlp.AddMediaToCss(css, cssClassName, MediaExtensions);

			// hack чтобы в IE скрывалась оригинальная стрелочка
			if (this.GetExtended("appearance") as string == "none")
			{
				HtmlHlp.AddClassToCss(css, ".{0}::-ms-expand", 
					new CssExtensionAttribute[] { new CssExtensionAttribute("display", "none") }
				);
			}

			HElement[] options = ArrayHlp.Convert(comboItems, delegate (Tuple<T, string> item)
      {
        object[] content = new object[] { h.value(item.Item1), item.Item2 };
        if (object.Equals(selected, item.Item1))
          content = ArrayHlp.Merge(content, new object[] { h.selected() });
        return h.Option(content);
      });

      return h.Select(HtmlHlp.ContentForHElement(this, cssClassName, h.data("name", Name), options)
      );
    }
  }

}
