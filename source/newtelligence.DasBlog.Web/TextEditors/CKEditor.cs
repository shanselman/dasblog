using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using newtelligence.DasBlog.Web.Core;

namespace newtelligence.DasBlog.Web.TextEditors
{
    public class CKEditor : EditControlAdapter
    {
        private TextBox control;

        public CKEditor()
        {

            control = new TextBox();
            control.TextMode = TextBoxMode.MultiLine;
            control.ID = "editor1";
        }

        public override void Initialize()
        {
            string ckeScriptUrl = "\"//cdn.ckeditor.com/4.4.7/standard/ckeditor.js\"";

            string insertCKEHandler = "<script language=\"javascript\" type=\"text/javascript\" src=" + ckeScriptUrl + "></script>";

            if (this.control.Visible)
            {
                insertCKEHandler += @"<script language=""javascript"" type=""text/javascript"">CKEDITOR.replace('EditUserBox$editor1');</script>";
            }

            this.Control.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "insertCKEHandler", insertCKEHandler);
        }

        public override bool HasText()
        {
            return !string.IsNullOrEmpty(this.Text);
        }


        public override Control Control
        {
            get { return control; }
        }

        public override string Text
        {
            get { return control.Text; }
            set { control.Text = value; }
        }

        public override Unit Width
        {
            get { return control.Width; }
            set { control.Width = value;  }
        }

        public override Unit Height
        {
            get { return control.Height; }
            set { control.Height = value; }
        }
    }
}