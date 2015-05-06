using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using FreeTextBoxControls;
using newtelligence.DasBlog.Web.Core;

namespace newtelligence.DasBlog.Web
{
	/// <summary>
	/// Converts the FreeTextBox control to the interface needed by dasBlog
	/// </summary>
	/// <remarks>
	/// Based on v3 of FreeTextBox http://www.freetextbox.com/
	/// </remarks>
	public class FreeTextBoxAdapter : EditControlAdapter
	{
		FreeTextBox freeTextBox;

		public FreeTextBoxAdapter()
		{
			freeTextBox = new FreeTextBox();
			freeTextBox.ID = "freeTextBox";
			freeTextBox.DesignModeCss = "ftb/designmode.css";
			freeTextBox.SupportFolder = "ftb/";
			freeTextBox.ToolbarLayout = "ParagraphMenu,FontFacesMenu,FontSizesMenu,FontForeColorsMenu|Bold,Italic,Underline,Strikethrough;Superscript,Subscript,RemoveFormat|JustifyLeft,JustifyRight,JustifyCenter,JustifyFull;BulletedList,NumberedList,Indent,Outdent;IeSpellCheck,CreateLink,Unlink,InsertImage,InsertImageFromGallery,InsertRule|Cut,Copy,Paste;Undo,Redo,Print";
			freeTextBox.ImageGalleryUrl = "ftb/ftb.imagegallery.aspx?rif={0}&amp;cif={0}";
			freeTextBox.RemoveServerNameFromUrls = false;
			freeTextBox.TextDirection = FreeTextBoxControls.TextDirection.LeftToRight;
			freeTextBox.ImageGalleryPath = SiteConfig.GetSiteConfig().BinariesDir;
		}

		public override Control Control { get { return freeTextBox; } }

		public override void Initialize()
		{
			base.Initialize ();
			if (this.freeTextBox.Visible && this.freeTextBox.EnableHtmlMode)
			{
				Toolbar myToolbar = new Toolbar();

				ToolbarButton myButton = new ToolbarButton("Insert Code", "insertCode", "csharp");

				string insertCodeCallback = @"<script type=""text/javascript"">
var ftbReference;
function setFtbReference(ftbRef){
	this.ftbReference = ftbRef;
}

function addTextToPost(html){
	ftbReference.InsertHtml(html);
}
</script>";
				this.Control.Page.ClientScript.RegisterClientScriptBlock(this.GetType(),"insertCodeCallback", insertCodeCallback);

				string scriptBlock = "var codescript = '" + this.freeTextBox.SupportFolder + @"ftb.insertcode.aspx';
if (FTB_Browser.isIE){
	code = showModalDialog(codescript,window,'dialogWidth:400px; dialogHeight:500px;help:0;status:0;resizeable:1;');
	if (code != null) {
		this.ftb.InsertHtml(code);
	}
} else {
	setFtbReference(this.ftb);
	window.open(codescript, 'insertcode', 'width=550,height=500,resizable=yes,scrollbars=yes,status=no,modal=yes,dependent=yes,dialog=yes');
}
";
				myButton.ScriptBlock = scriptBlock;
				myToolbar.Items.Add(myButton);
				this.freeTextBox.Toolbars.Add(myToolbar);
			}
		}


		public override string Text
		{
			get	{ return freeTextBox.Text; }
			set { freeTextBox.Text = value; }
		}

		public override bool HasText()
		{
			return (freeTextBox.Text.Trim().Length > 0 && freeTextBox.Text.Trim() != "<p>\r\n\t\t</p>");
		}

		public override Unit Width
		{
			get { return freeTextBox.Width; }
			set { freeTextBox.Width = value; }
		}

		public override Unit Height
		{
			get { return freeTextBox.Height; }
			set { freeTextBox.Height = value; }
		}

		public override void SetLanguage(string language)
		{
			freeTextBox.Language = ConvertToSupportedLanguage(language);
		}

		public override void SetTextDirection(SharedBasePage.TextDirection textDirection)
		{
			if (textDirection == SharedBasePage.TextDirection.RightToLeft) 
			{
				freeTextBox.TextDirection = FreeTextBoxControls.TextDirection.RightToLeft;
			}
			else // default to LTR
			{
				freeTextBox.TextDirection = FreeTextBoxControls.TextDirection.LeftToRight;
			}
		}

		// could be a performance issue, but the editor is only used during editing 
		// so we should be ok, in the 90% case.
		// If it becomes an issue we should cache the supported cultures.
		private static string ConvertToSupportedLanguage(string language ){

			if( language == null ){
				return "en-US";
			}

			FreeTextBoxControls.Support.ResourceManager rm = new FreeTextBoxControls.Support.ResourceManager();
			NameValueCollection coll = rm.GetSupportedLanguages();

			string[]  cultures = new string[coll.Count];

			coll.CopyTo(cultures,0);

			int index = Array.IndexOf( cultures, language );

			if( index > -1){
				return language;
			}

			// convert to a CultureInfo object, so we can find the fall back culture
			CultureInfo reqCulture;
			try{
				reqCulture = new CultureInfo( language ); 
			}catch(Exception){
				return "en-US";
			}

			CultureInfo parent = (!reqCulture.IsNeutralCulture? reqCulture.Parent: reqCulture);
			
			foreach( string s in cultures ){
				CultureInfo current = new CultureInfo(s).Parent;

				// same parent should be good enough
				if( parent.Equals(current) ){
					return s;
				}
			}
			
			// if we don't support your language, we return en-us
			return "en-US";
		}
	}
}
