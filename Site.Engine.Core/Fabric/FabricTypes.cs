using Commune.Basis;
using Commune.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Engine
{
	public class SEOProp
	{
		public readonly static JsonUniqueBlank<NameId> Identifier = NameId.Blank;

		public readonly static PropertyBlank<string> Title = new(101, DataBox.StringValue);
		public readonly static PropertyBlank<string> Description = new(102, DataBox.StringValue);
		public readonly static PropertyBlank<string> Keywords = new(103, DataBox.StringValue);

		public readonly static PropertyBlank<string> Text = new(104, DataBox.StringValue);

		public readonly static PropertyBlank<string> Name = new(110, DataBox.StringValue);
		public readonly static PropertyBlank<bool> IsImport = new(115, DataBox.BoolValue);

		public readonly static PropertyBlank<string> SortingPrefix = new(1100, DataBox.StringValue);
		public readonly static PropertyBlank<DateTime?> SortTime = new(17103, DataBox.DateTimeNullableValue);

		public static string GetDisplayName(LightObject obj)
		{
			string name = obj.Get(SEOProp.Name);
			if (!StringHlp.IsEmpty(name))
				return name;

			return NameId.GetName(obj);
		}

		public static string GetEditName(LightObject obj)
		{
			string identifier = NameId.GetName(obj);
			string name = obj.Get(SEOProp.Name);
			if (StringHlp.IsEmpty(name))
				return identifier;

			return string.Format("{0} ({1})", name, identifier);
		}
	}

	public class NewsType
	{
		public const int News = 5000;

		public readonly static JsonUniqueBlank<NameId> Title = NameId.Blank;

		public readonly static PropertyBlank<string> Annotation = new(5101, DataBox.StringValue);
		public readonly static PropertyBlank<string> Text = new(5102, DataBox.StringValue);

		public readonly static PropertyBlank<bool> IsLink = new(5110, DataBox.BoolValue);
		public readonly static PropertyBlank<string> LinkUrl = new(5115, DataBox.StringValue);

		public readonly static PropertyBlank<int> PublisherId = new(5121, DataBox.IntValue);
		public readonly static PropertyBlank<string> OriginName = new(5122, DataBox.StringValue);
		public readonly static PropertyBlank<string> OriginUrl = new(5123, DataBox.StringValue);
	}


	public class ContactsType
	{
		public const int Contacts = 9000;

		public readonly static JsonUniqueBlank<KindId> Kind = KindId.Blank;

		public readonly static PropertyBlank<string> Brand = new(9101, DataBox.StringValue);
		public readonly static PropertyBlank<string> Address = new(9102, DataBox.StringValue);
		public readonly static PropertyBlank<string> Phones = new(9103, DataBox.StringValue);
		public readonly static PropertyBlank<string> Email = new(9104, DataBox.StringValue);

		public readonly static PropertyBlank<string> Header = new(9105, DataBox.StringValue);
		public readonly static PropertyBlank<string> About = new(9106, DataBox.StringValue);

		public readonly static PropertyBlank<string> SocialNetwork = new(9107, DataBox.StringValue);

		public readonly static PropertyBlank<string> MapWidget = new(9108, DataBox.StringValue);

		public readonly static PropertyBlank<string> Alert = new(9110, DataBox.StringValue);
	}

	public class SEOType
	{
		public const int SEO = 10000;

		public readonly static JsonUniqueBlank<KindId> Kind = KindId.Blank;

		public readonly static PropertyBlank<string> MainTitle = new(10101, DataBox.StringValue);
		public readonly static PropertyBlank<string> MainDescription = new(10102, DataBox.StringValue);
		public readonly static PropertyBlank<string> MainKeywords = new(10103, DataBox.StringValue);

		public readonly static PropertyBlank<string> FooterSeoText = new(10104, DataBox.StringValue);

		public readonly static PropertyBlank<string> SectionTitlePattern = new(10111, DataBox.StringValue);
		public readonly static PropertyBlank<string> SectionDescriptionPattern = new(10112, DataBox.StringValue);

		//public readonly static PropertyBlank<string> GroupTitlePattern = new(10201, DataBox.StringValue);
		//public readonly static PropertyBlank<string> GroupDescriptionPattern = new(10202, DataBox.StringValue);

		//public readonly static PropertyBlank<string> ProductTitlePattern = new(10301, DataBox.StringValue);
		//public readonly static PropertyBlank<string> ProductDescriptionPattern = new(10302, DataBox.StringValue);
	}

	public class RedirectType
	{
		public const int Redirect = 12000;

		public readonly static JsonUniqueBlank<NameId> From = NameId.Blank;

		public readonly static PropertyBlank<string> To = new(12101, DataBox.StringValue);
	}

	public class SEOWidgetType
	{
		public const int Widget = 13000;

		public readonly static JsonUniqueBlank<NameId> DisplayName = NameId.Blank;

		public readonly static PropertyBlank<string> Code = new(13101, DataBox.StringValue);
	}

	public class SectionType
	{
		public const int Section = 17000;

		public readonly static JsonUniqueBlank<NameId> Title = NameId.Blank;

		public readonly static PropertyBlank<string> DesignKind = new(17101, DataBox.StringValue);
		public readonly static PropertyBlank<string> SortKind = new(17102, DataBox.StringValue);
		public readonly static PropertyBlank<string> Tags = new(17104, DataBox.StringValue);
		public readonly static PropertyBlank<string> UnitSortKind = new(17105, DataBox.StringValue);
		public readonly static PropertyBlank<bool> HideInMenu = new(17106, DataBox.BoolValue);

		public readonly static PropertyBlank<string> NameInMenu = new(17110, DataBox.StringValue);
		public readonly static PropertyBlank<string> Subtitle = new(17111, DataBox.StringValue);
		public readonly static PropertyBlank<string> Annotation = new(17120, DataBox.StringValue);

		public readonly static PropertyBlank<string> Link = new(17121, DataBox.StringValue);


		public readonly static PropertyBlank<string> Content = new(17200, DataBox.StringValue);
		public readonly static PropertyBlank<string> Widget = new(17210, DataBox.StringValue);
		public readonly static PropertyBlank<string> UnderContent = new(17220, DataBox.StringValue);

		public readonly static LinkBlank SubsectionLinks = new(17500);
		public readonly static LinkBlank UnitForPaneLinks = new(17509);
	}

	public class UnitType
	{
		public const int Unit = 18000;

		public readonly static JsonUniqueBlank<ParentNameId> JsonId = ParentNameId.Blank;

		//public readonly static XmlParentDisplayName<int> ParentId = XmlParentDisplayName<int>.ParentId;
		//public readonly static XmlParentDisplayName<string> DisplayName = XmlParentDisplayName<string>.DisplayName;

		public readonly static PropertyBlank<string> DesignKind = SectionType.DesignKind;

		public readonly static PropertyBlank<string> ImageAlt = new(18101, DataBox.StringValue);
		public readonly static PropertyBlank<string> AdaptTitle = new(18102, DataBox.StringValue);
		public readonly static PropertyBlank<string> Tags = new(18104, DataBox.StringValue);

		public readonly static PropertyBlank<string> Annotation = new(18110, DataBox.StringValue);
		public readonly static PropertyBlank<string> Subtitle = new(18111, DataBox.StringValue);
		public readonly static PropertyBlank<string> Link = new(18120, DataBox.StringValue);

		public readonly static PropertyBlank<string> Content = new(18130, DataBox.StringValue);
		
		public readonly static PropertyBlank<string> SortKind = new(18140, DataBox.StringValue);

		public readonly static LinkBlank SubunitLinks = new(18500);
	}

}
