using System;
using RJH.CommandLineHelper;

namespace DasBlog.Import
{
	/// <summary>
	/// Summary description for EntryPoint.
	/// </summary>
	public class EntryPoint
	{
	
		public static CommandLine CommandLine
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get 
			{ 
				return m_CommandLine; 
			}  
			[System.Diagnostics.DebuggerStepThrough()]
			set
			{ 
				m_CommandLine = value; 
			}  
		} 
		private static CommandLine m_CommandLine;

		/// <summary>
		/// This is the entry point into the DLL.  It parses the command line and
		/// delegates the work out to each program.
		/// </summary>
		/// <param name="commandLine">The commandline that invoked the program.  Usually
		/// this corresponds to System.Environment.CommandLine</param>
		/// <returns></returns>
		public static void DllMain(string commandLine)
		{
			CommandLine = new CommandLine();
			Parser parser = new Parser(commandLine, CommandLine);			
			parser.Parse();

			if(CommandLine.Help)
			{
				Console.WriteLine("This program is used to import blog data from other blogging programs.");
				Console.WriteLine("  Note that this program will modify your content directory so back it up.");
				Console.WriteLine("  Also, the program will require an internet connection in some instances, ");
				Console.WriteLine("  like importing comments from an external server.");
				Console.WriteLine( parser.GetHelpText() );
			}
			else
			{
				switch(CommandLine.Source)
				{
					case BlogSourceType.Radio:
						if(CommandLine.SourceDirectory !=  null && CommandLine.SourceDirectory.Length > 0
							&& CommandLine.ContentDirectory != null && CommandLine.ContentDirectory.Length > 0)
						{
							Console.WriteLine("Importing entries from Radio...");
							DasBlog.Import.Radio.EntryImporter.Import(
								CommandLine.SourceDirectory,
								CommandLine.ContentDirectory);
						}
						else
						{
							Console.WriteLine("Entries from Radio not imported because either source directory or content directory were not specified.");
						}
						
						if(CommandLine.UserID != null && CommandLine.UserID.Length > 0 
							&& CommandLine.ContentDirectory != null && CommandLine.ContentDirectory.Length > 0)

						{
							if(CommandLine.CommentServer != null && CommandLine.CommentServer.Length > 0)
							{
								Console.WriteLine("Defaulting to comment server {0}.  You may need to check your radio source.  radiocomments2, etc",DasBlog.Import.Radio.CommentImporter.DefaultCommentServer);
							}

							Console.WriteLine("BETA: Importing comments from Radio...");
							DasBlog.Import.Radio.CommentImporter.Import(
								CommandLine.UserID, CommandLine.ContentDirectory, CommandLine.CommentServer );
						}
						else
						{
							Console.WriteLine("Comments from Radio not imported because comment server, userid or content directory were not specified.");
						}


						break;
					case BlogSourceType.none:
						goto default;
					default:
						throw new ApplicationException(
							string.Format("The source option was not specified or else was invalid."));
				}
			}
		}
	}
}
