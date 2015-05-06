/* Based on example at http://wiki.fckeditor.net/Developer%27s_Guide/Customization/Plug-ins
 * Provides pretty-print code support to FCKeditor
 */

/* hacky way to get website path */ 
var websiteRoot = FCKConfig.BasePath.substr(0, FCKConfig.BasePath.length - 'FCKeditor/editor/'.length);

var InsertCodeCommand=function(){
        //create our own command, we dont want to use the FCKDialogCommand 
        // because it uses the default fck layout and not our own
};
InsertCodeCommand.prototype.Execute=function(){
        
}
InsertCodeCommand.GetState=function() {
        return FCK_TRISTATE_OFF; //we dont want the button to be toggled
}

InsertCodeCommand.Execute=function() {
        //open a popup window when the button is clicked
	var codescript = websiteRoot + 'ftb/ftb.insertcode.aspx';
	if (window.showModalDialog){
		code = showModalDialog(codescript,window,'dialogWidth:400px; dialogHeight:500px;help:0;status:0;resizeable:1;');
		if (code != null) {
			addTextToPost( code );
		}
	} else {
		window.open(codescript, 'insertcode', 'width=550,height=500,resizable=yes,scrollbars=yes,status=no,modal=yes,dependent=yes,dialog=yes');
	}
}

function addTextToPost(html){
	// Get the editor instance that we want to interact with.
	var oEditor = FCK;

	// Check the active editing mode.
	if ( oEditor.EditMode == FCK_EDITMODE_WYSIWYG )
	{
		// Insert the desired HTML.
		oEditor.InsertHtml( html ) ;
	}
	else
		alert( 'You must be on WYSIWYG mode!' ) ;
}

FCKCommands.RegisterCommand('Insert_Code', InsertCodeCommand );

var oInsertCode = new FCKToolbarButton('Insert_Code', FCKLang.InsertCodeBtn);
oInsertCode.IconPath = websiteRoot + 'ftb/images/csharp.gif'; //borrow the FTB image

FCKToolbarItems.RegisterItem( 'Insert_Code', oInsertCode );