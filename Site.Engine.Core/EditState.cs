using Commune.Html;
using Commune.Data;
using NitroBolt.Wui;

namespace Site.Engine
{
  public class EditState : IWuiState
  {
		public WuiCallKind CallKind { get; set; }

		public readonly WebOperation Operation = new WebOperation();

    //hack чтобы можно было редактировать только что созданный объект
    public int? CreatingObjectId = null;

		// панель изображений
		public bool AllowImageDeletion = false;

		// галерея
		public int? MovableImageIndex = null;
		public bool AllowDeleteImage = false;

		volatile string lastJson = "";
		public bool IsRattling(JsonData json)
		{
			if (json?.JPath("data", "command") == null)
				return true;

			string jsonAsStr = json.ToString();
			if (lastJson == jsonAsStr)
				return true;

			lastJson = jsonAsStr;
			return false;
		}

		public string PopupDialog = "";

		public volatile object? Tag = null;
	}

	public class EnginePanelResult
	{
		public IHtmlControl Panel;
		public string Title;

		public EnginePanelResult(IHtmlControl panel, string title)
		{
			this.Panel = panel;
			this.Title = title;
		}
	}

}
