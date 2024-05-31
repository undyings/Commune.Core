using Commune.Basis;
using NitroBolt.Wui;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Html
{
  public static class std
  {
    public static IHtmlControl Upbutton(string imageUrl)
    {
      return new HLink("#",
          new HImage(imageUrl)
      ).ExtraClassNames("upbutton").Display("none").Title("Наверх")
        .BoxSizing().Size(46, 46).Align(null).PaddingTop(13).FontSize(20)
        .Background("#fff").BorderRadius("50%")
        .BoxShadow("0 5px 10px rgba(0,0,0,.4), 0 -1px 1px rgba(0,0,0,.2)")
        .ZIndex(500).Bottom(10).Right(10)
        .Media(480, new HStyle().Size(36, 36).Bottom(3).Right(3).PaddingTop(8));
    }

    public static HClickDropdown ComboButton(HBefore beforeIcon, string caption, 
      bool isLeftDropListAlignment, params IHtmlControl[] listControls)
    {
      return new HClickDropdown(
        new HDropStyle().
          DropList(new HTone()
            .Top("31px")
            .CssAttribute(isLeftDropListAlignment ? "left" : "right", "0px")
            .Background("#f7f7f7")
            .Border("1px", "solid", "#e0e0e0", "2px")
            .CssAttribute("border-top", "none")
            .Padding(4, 0)
          ).
          RootWhenDropped(new HTone()
            .Background("#f7f7f7")
            .Border("1px", "solid", "#e0e0e0", "0px")
            .CssAttribute("border-bottom", "none")
          ).
          AnyItem(new HTone().Padding(4, 8)).
          SelectedItem(new HTone()
            .Color("#f5ffe6")
            .Background("#999999")
          ),
        new HButton(caption,
          beforeIcon,
          new HAfter().Content(@"▼").FontSize("60%").MarginLeft(5).MarginRight(-2).VAlign(1),
          new HHover().Border("1px", "solid", "#aaaaaa", "2px")
            .Background("#eaeaea")
            .LinearGradient("to top right", "#cccccc", "#eaeaea")
        ).Padding(6, 12)
          .Background("#f1f1f1")
          .LinearGradient("to top right", "#dddddd", "#f1f1f1")
          .Border("1px", "solid", "#bbbbbb", "2px"),
        listControls
      );
    }

    public static HSeparator Separator()
    {
      return new HSeparator().CssAttribute("border-bottom", "1px solid #fff").Height("1px")
        .Background("#c9c9c9").Margin(2, 0, 1, 0);
    }

    public static HLabel Label(string caption)
    {
      return new HLabel(caption).Padding("4px 8px");
    }

    public static HButton Button(string caption, params HStyle[] prefixStyles)
    {
      return Button(caption, 6, 12, prefixStyles);
    }

    public static HButton Button(string caption, int vertPadding, int horPadding, params HStyle[] prefixStyles)
    {
      HStyle hover = new HStyle(".{0}:hover")
        .Border("1px", "solid", "#aaaaaa", "2px")
        .Background("#eaeaea")
        .LinearGradient("to top right", "#cccccc", "#eaeaea");

      HStyle active = new HStyle(".{0}:active")
        .Border("2px", "double", "#2c628b", "2px")
        .Padding(vertPadding - 1, horPadding, vertPadding - 1, horPadding - 2)
        .Background("#e5f4fc")
        .LinearGradient("to top right", "#68b3db", "#e5f4fc");

      HStyle[] allStyles = new HStyle[] { hover, active };
      if (prefixStyles.Length != 0)
        allStyles = ArrayHlp.Merge(allStyles, prefixStyles);

      return new HButton(caption, allStyles)
        .Color("black")
        .Padding(vertPadding, horPadding)
        .Border("1px", "solid", "#bbbbbb", "2px")
        .Background("#f1f1f1")
        .LinearGradient("to top right", "#dddddd", "#f1f1f1");
    }

    public static HBefore BeforeAwesome(string content, bool regular, int marginRight)
    {
      return new HBefore().Content(content).FontFamily("FontAwesome").FontWeight(regular ? 400 : 900).MarginRight(marginRight);
    }

    public static HBefore BeforeAwesome(string content, int marginRight)
    {
      return BeforeAwesome(content, false, marginRight);
    }

		public static HAfter AfterAwesome(string content, bool regular, int marginLeft)
    {
      return new HAfter().Content(content).FontFamily("FontAwesome").FontWeight(regular ? 400 : 900).MarginLeft(marginLeft);
    }

    public static HAfter AfterAwesome(string content, int marginLeft)
    {
      return AfterAwesome(content, false, marginLeft);
    }

    public static HBefore DialogIconAwesome(DialogIcon? iconKind)
    {
      switch (iconKind)
      {
				case DialogIcon.Info:
					return std.BeforeAwesome(@"\f05a", 0);
				case DialogIcon.Warning:
					return std.BeforeAwesome(@"\f071", 0);
				case DialogIcon.Question:
					return std.BeforeAwesome(@"\f059", 0);
				case DialogIcon.Error:
					return std.BeforeAwesome(@"\f28e", 0);
				case DialogIcon.Success:
					return std.BeforeAwesome(@"\f00c", 0);
				default:
					return new HBefore();
			}

		}

    public static void AddStyleForFileUploaderButtons(StringBuilder css)
    {
      HtmlHlp.AddStyleToCss(css, "qq-upload-button", new HStyle()
        .InlineBlock()
        .Padding(6, 12)
        .Border("1px", "solid", "#bbbbbb", "2px")
        .Background("#f1f1f1")
        .LinearGradient("to top right", "#dddddd", "#f1f1f1")
      );

      HtmlHlp.AddStyleToCss(css, "qq-upload-button-hover", new HStyle()
        .Border("1px", "solid", "#aaaaaa", "2px")
        .Background("#eaeaea")
        .LinearGradient("to top right", "#cccccc", "#eaeaea")
      );
    }


    public static HPanel OperationWarning(WebOperation operation)
    {
      string titleColor = "#FFF"; // "#F1F6CD";
      string titleBackground = "#880000"; // "#CC0000"; // "#597BA5";
      return new HPanel(
        new HPanel(
          new HLabel("",
            new HBefore().Content(@"\f0e7")
              .Color(titleColor).Opacity("0.6").FontFamily("FontAwesome").FontBold()
          ).Cursor("default"),
          new HLabel(operation.Message).MarginLeft(8)
            .Color(titleColor).FontFamily("Arial").FontSize("13px").FontBold(),
          new HButton("",
            new HAfter().Content(@"\f00d")
              .FontFamily("FontAwesome").FontSize("1.15em"),
            new HHover().Opacity("1")
          ).FloatRight().MarginTop(-1).Color("#FFF").Opacity("0.6")
            .Event("warning_hide", "", delegate (JsonData json)
            {
              operation.Reset();
            })
        ).Padding(6, 10, 7, 14).Background(titleBackground).BorderRadius(16)
      ).Hide(operation.Completed || StringHlp.IsEmpty(operation.Message))
      .MarginTop(12);
    }
  }
}
