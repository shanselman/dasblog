using System.Web.UI;
using System.Web.UI.WebControls;
using newtelligence.DasBlog.Web.Core;

namespace newtelligence.DasBlog.Contrib
{
	/// <summary>
	/// Converts the TinyMCEeditor control to the interface needed by dasBlog
	/// </summary>
	/// <remarks>
	/// First written for v2.0.8 of TinyMCE 
	/// Updated to use TinyMCE 2.0.9
	/// http://tinymce.moxiecode.com/
	/// </remarks>

	public class TinyMCEeditorAdapter : EditControlAdapter
	{
		TextBox _Control;

		public TinyMCEeditorAdapter()
		{
			_Control = new TextBox();
			_Control.ID = "editControl";
			_Control.TextMode = TextBoxMode.MultiLine;
			_Control.CssClass = "mceEditor";
		}

		public override void Initialize()
		{

			string TinyMCEPath = "tinymce/jscripts/tiny_mce/tiny_mce.js";

			// if you are using this for the public, this is the only reasonable way to use MCE
		    //string TinyMCEPath = "tinymce/jscripts/tiny_mce/tiny_mce_gzip.js";
			string insertMCEHandler = "";
			insertMCEHandler = "<script language=\"javascript\" type=\"text/javascript\" src=" + TinyMCEPath + "></script>";


			if (this._Control.Visible)
			{

				insertMCEHandler += @"
				<script language=""javascript"" type=""text/javascript"">
					tinyMCE_GZ.init({
					plugins : 'nonbreaking,paste',
					themes : 'advanced',
					languages : 'en',
					disk_cache : true,
					debug : false
					});
					</script>";

				//theme_advanced_buttons1_add_before : ""save,newdocument,separator"",
				//plugin_insertdate_dateFormat : ""%Y-%m-%d"",
				//plugin_insertdate_timeFormat : ""%H:%M:%S"",
				//	external_link_list_url : ""example_link_list.js"",
				//	external_image_list_url : ""example_image_list.js"",
				//	flash_external_list_url : ""example_flash_list.js"",
				//	media_external_list_url : ""example_media_list.js"",
				// insertdate,inserttime,
				// devkit,

				insertMCEHandler += @"
				<script language=""javascript"" type=""text/javascript"">
				tinyMCE.init({
					mode: ""specific_textareas"",
					editor_selector : ""mceEditor"",
					theme : ""advanced"",
					plugins : ""style,layer,table,save,advhr,emotions,iespell,insertdatetime,preview,media,searchreplace,print,contextmenu,paste,directionality,fullscreen,noneditable,visualchars,nonbreaking,xhtmlxtras"",
					
					theme_advanced_buttons1_add : ""fontselect,fontsizeselect"",
					theme_advanced_buttons2_add : ""separator,forecolor,backcolor,advsearchreplace"",
					theme_advanced_buttons2_add_before: ""emotions,iespell,media,advhr,separator"",
					theme_advanced_buttons3_add_before : ""insertlayer,moveforward,movebackward,absolute,|,styleprops,|,tablecontrols,separator"",
					theme_advanced_buttons3_add : """",
					theme_advanced_buttons4 : ""ltr,rtl,cut,copy,paste,pastetext,pasteword,separator,search,replace,separator,cite,abbr,acronym,del,ins,|,visualchars,nonbreaking,separator,fullscreen,separator,print,separator,preview,cleanup,help,code"",
					theme_advanced_toolbar_location : ""top"",
					theme_advanced_toolbar_align : ""left"",
					theme_advanced_path_location : ""bottom"",
					content_css : ""tinymce/content.css"",
					extended_valid_elements : ""hr[class|width|size|noshade],font[face|size|color|style],span[class|align|style]"",
					file_browser_callback : ""fileBrowserCallBack"",
					theme_advanced_resize_horizontal : false,
					theme_advanced_resizing : true,
					nonbreaking_force_tab : true,
					gecko_spellcheck : true,
					plugin_preview_width : ""600"",
					plugin_preview_height : ""400"",
					force_br_newlines : true,
					apply_source_formatting : true
				});

																					
				</script>";
				//freeTextBox.ImageGalleryUrl = "ftb/ftb.imagegallery.aspx?rif={0}&amp;cif={0}";


				Control.Page.ClientScript.RegisterClientScriptBlock(GetType(), "insertMCEHandler", insertMCEHandler, false);



			}
		}		
	

		public override Control Control
		{
			get
			{
				return _Control;
			}
		}

		public override string Text 
		{ 
			get { return _Control.Text; }
			set { _Control.Text = value; }
		}

		public override bool HasText() 
		{
			return (_Control.Text.Trim().Length > 0 && _Control.Text.Trim() != "<p></p>");
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

	}
}
