/*
 http://wiki.fckeditor.net/Developer%27s_Guide/Configuration/Configurations_File

 The custom configuration file does not seem to be working, so I made the
 toolbar changes directly in /FCKeditor/fckconfig.js
*/

FCKConfig.UseBROnCarriageReturn	= true ;

FCKConfig.Plugins.Add( 'insertcode', 'en' );
   
FCKConfig.ToolbarSets["Default"] = [
	['Source','Preview'],
	['Cut','Copy','Paste','PasteText','PasteWord','-','SpellCheck'],
	['Undo','Redo','-','Find','Replace','-','SelectAll','RemoveFormat'],
	['Bold','Italic','Underline','StrikeThrough','-','Subscript','Superscript'],
	['OrderedList','UnorderedList','-','Outdent','Indent'],
	['JustifyLeft','JustifyCenter','JustifyRight','JustifyFull'],
	['Link','Unlink','Anchor'],
	['Table','Rule','Smiley','SpecialChar','UniversalKey', 'Insert_Code'],
	'/',
	['Style','FontFormat','FontName','FontSize'],
	['TextColor','BGColor'],
	['About']
] ;

FCKConfig.ToolbarSets["Basic"] = [
	['Bold','Italic','-','OrderedList','UnorderedList','-','Link','Unlink','-','About']
] ;
