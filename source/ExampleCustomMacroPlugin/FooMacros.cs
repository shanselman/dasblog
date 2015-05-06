using System;
using System.Web;
using System.Web.UI;
using newtelligence.DasBlog.Runtime;
using newtelligence.DasBlog.Web.Core;

	// In your web.config you need to create a mapping between a name and 
	// the .NET type that handles it. 
	//
	// web.config:
	//	
	// <configSections>
	//    <section name="newtelligence.DasBlog.Macros" type="newtelligence.DasBlog.Web.Core.MacroSectionHandler, newtelligence.DasBlog.Web.Core" />
	// </configSections>
	// <newtelligence.DasBlog.Macros>
	//	   <!-- The format for a QN is Typename,AssemblyName. Do NOT include the .dll extension. -->
	//     <add macro="foo" type="FooMacroPlugin.FooMacros,FooMacroPlugin"/> 
	// </newtelligence.DasBlog.Macros>
	// 
	// Then your macro DLL would go in your \BIN folder. If you need to debug, include your PDB files.
	//
	// Within your homeTemplate.blogtemplate or itemTemplate.blogtemplate you'd use the macro like this.
	// They are not fully supported in dayTemplate.blogtemplate. Be aware.
	//
	// <%TestControl(“here is some text”)|foo%>                    
	// <%TestControlWithinEntry()|foo%>
	//
	
namespace FooMacroPlugin
{
	/// <summary>
	/// This is an example of how to write a custom macro.
	/// </summary>
	public class FooMacros 
	{
		protected SharedBasePage requestPage;
		protected Entry currentItem;

		public FooMacros(SharedBasePage page, Entry item)
		{
			requestPage = page;
			currentItem = item;
		}

		//be sure to check and see that currentItem and/or requestPage are NOT NULL before you use them!
		public virtual Control TestControl(string text)
		{
			return new LiteralControl("There are " + requestPage.WeblogEntries.Count  + " entries on this page");
		}

		//be sure to check and see that currentItem and/or requestPage are NOT NULL before you use them!
		public virtual Control TestControlWithinEntry()
		{
			return new LiteralControl("The Title of this entry is " + currentItem.Title);
		}

	}
}
