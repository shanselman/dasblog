// Copyright (c) 2003 Craig Andera
// TODO: Add appropriate license
// TODO: Change copyright if necessary

using System;
using System.IO; 
using System.Xml; 
using System.Xml.XPath; 
using System.Collections; 

using newtelligence.DasBlog.Runtime; 

namespace Candera.DasBlog.BlogXContentImport
{
	class App
	{
		private const int SUCCESS = 0; 
		private const int ERRORWRONGUSAGE = -1; 
		private const int ERRORINPUTDIRNOTFOUND = -2; 
		private const int ERROROUTPUTDIRNOTFOUND = -3; 
		private const int ERROREXCEPTION = -4; 


		static int Main(string[] args)
		{
			try
			{
				#region Command Line Parsing
				string inputdir = null; 
				string outputdir = null; 
				for (int i = 0; i < args.Length; ++i)
				{
					if (args[i] == "-inputdir")
					{
						inputdir = args[++i]; 
					}
					else if (args[i] == "-outputdir")
					{
						outputdir = args[++i]; 
					}
				}

				if (inputdir == null || outputdir == null)
				{
					PrintUsage(); 
					return ERRORWRONGUSAGE; 
				}

				// Canonicalize and expand path to full path
				inputdir = Path.GetFullPath(inputdir); 
				outputdir = Path.GetFullPath(outputdir); 

				if (!Directory.Exists(inputdir))
				{
					Console.WriteLine(inputdir + " does not exist or is not a directory"); 
					return ERRORINPUTDIRNOTFOUND; 
				}

				if (!Directory.Exists(outputdir))
				{
					Console.WriteLine(outputdir + " does not exist or is not a directory"); 
					return ERROROUTPUTDIRNOTFOUND; 
				}

				#endregion Command Line Parsing

				IBlogDataService dsInput = BlogDataServiceFactory.GetService(inputdir,null);
				IBlogDataService dsOutput = BlogDataServiceFactory.GetService(outputdir, null); 

				Console.WriteLine("Porting posts"); 
				// Copy all dayentry files to output directory
				// This shouldn't require any conversion, since the format and naming convention matches
				// between BlogX and dasBlog
				EntryCollection entries =	dsInput.GetEntriesForDay(
					DateTime.MaxValue.AddDays(-2),
					TimeZone.CurrentTimeZone,
					String.Empty,
					int.MaxValue,
					int.MaxValue,
					String.Empty);

				//Hashtable lookup = new Hashtable(); 
				foreach (Entry e in entries)
				{
					//lookup[e.EntryId] = e; 
					dsOutput.SaveEntry(e); 
					Console.Write("."); 
				}

				Console.WriteLine(); 
				Console.WriteLine("Posts successfully ported"); 

				Console.WriteLine("Porting comments"); 

				// TODO: Read in all dayextra files from input directory
				int commentCount = 0; 
				string[] commentFiles = Directory.GetFiles(inputdir, "*.dayextra.xml");
				foreach (string commentFile in commentFiles)
				{
					// TODO: Match up comments with DayEntry, emit comment
					XPathDocument doc = new XPathDocument(Path.Combine(inputdir, commentFile)); 
					XPathNavigator nav = doc.CreateNavigator(); 
					XPathNodeIterator commentNodes = nav.Select("//Comment"); 

					while (commentNodes.MoveNext())
					{
						Comment comment = new Comment(); 
            
						XPathNavigator commentNode = commentNodes.Current; 

						comment.Content = (string) commentNode.Evaluate("string(Content)"); 
						comment.CreatedUtc = DateTime.Parse((string) commentNode.Evaluate("string(Created)")); 
						comment.ModifiedUtc = DateTime.Parse((string) commentNode.Evaluate("string(Modified)")); 
						comment.EntryId = (string) commentNode.Evaluate("string(EntryId)"); 
						comment.TargetEntryId = (string) commentNode.Evaluate("string(TargetEntryId)"); 
						comment.Author = (string) commentNode.Evaluate("string(Author)"); 
						comment.AuthorEmail = (string) commentNode.Evaluate("string(AuthorEmail)"); 
						comment.AuthorHomepage = (string) commentNode.Evaluate("string(AuthorHomepage)"); 

						dsOutput.AddComment(comment); 
						Console.Write("."); 
						++commentCount; 
					}
				}

				Console.WriteLine(); 
				Console.WriteLine("{0} comments successfully imported!", commentCount); 
			}
			catch (Exception e)
			{
				// Return nonzero so automated tools can tell it failed
				Console.WriteLine(e.ToString()); 
				return ERROREXCEPTION; 
			}

			return SUCCESS; 
		}

		public static void PrintUsage()
		{
			Console.WriteLine(@"
DasBlogBlogXContentImport
Copyright (c) 2003 Craig Andera http://staff.develop.com/candera

Usage: 

dasblogblogxcontentimport -inputdir <input directory> -outputdir <output directory> 

  <input directory>   - Directory where BlogX dayentry and dayextra files live (must exist)
  <output directory>  - Directory where dasBlog dayentry and dayextra files will be created (must exist)
");
		}
	}
}
