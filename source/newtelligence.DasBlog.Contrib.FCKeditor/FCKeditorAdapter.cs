using System.Web.UI;
using System.Web.UI.WebControls;
using FredCK.FCKeditorV2;
using newtelligence.DasBlog.Web.Core;

namespace newtelligence.DasBlog.Contrib
{
	/// <summary>
	/// Converts the FCKeditor control to the interface needed by dasBlog
	/// </summary>
	/// <remarks>
	/// Based on v2.2 of FCKeditor and FCKeditor.NET 
	/// http://www.fckeditor.net/
	/// http://sourceforge.net/project/showfiles.php?group_id=75348&package_id=137125
	/// </remarks>
	public class FCKeditorAdapter : EditControlAdapter
	{
		FCKeditor _Control;

		public FCKeditorAdapter()
		{
			_Control = new FCKeditor();
			_Control.BasePath = "~/FCKeditor/";
		}

		public override Control Control { get { return _Control; } }

		public override void Initialize()
		{
			_Control.CustomConfigurationsPath = _Control.ResolveUrl("~/SiteConfig/FCKeditorConfig.js");
			_Control.StylesXmlPath = _Control.ResolveUrl("~/SiteConfig/FCKeditorStyles.xml");
		}

		public override string Text
		{
			get { return _Control.Value; }
			set { _Control.Value = value; } }

		public override bool HasText()
		{
			return (_Control.Value.Trim().Length > 0 && _Control.Value.Trim() != "<p>&nbsp;</p>");
		}

		public override Unit Width
		{
			get { return _Control.Width; }
			set { _Control.Width = value; }
		}

		public override Unit Height
		{
			get { return _Control.Height; }
			set { _Control.Height = value; }
		}

		public override void SetLanguage(string language)
		{
			_Control.AutoDetectLanguage = false;
			_Control.DefaultLanguage = language;
		}

		public override void SetTextDirection(SharedBasePage.TextDirection textDirection)
		{
			if (textDirection == SharedBasePage.TextDirection.RightToLeft)
			{
				_Control.ContentLangDirection = LanguageDirection.RightToLeft;
			}
			else // default to LTR
			{
				_Control.ContentLangDirection = LanguageDirection.LeftToRight;
			}

		}



	}
}
