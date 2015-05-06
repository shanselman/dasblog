#region	Copyright (c) 2003, newtelligence AG. All rights reserved.
/*
// Copyright (c) 2003, newtelligence AG. (http://www.newtelligence.com)
// Original	BlogX Source Code: Copyright (c) 2003, Chris Anderson (http://simplegeek.com)
// All rights reserved.
//	
// Redistribution and use in source	and	binary forms, with or without modification,	are	permitted 
// provided	that the following conditions are met: 
//	
// (1) Redistributions of source code must retain the above	copyright notice, this list	of 
// conditions and the following	disclaimer.	
// (2) Redistributions in binary form must reproduce the above copyright notice, this list of 
// conditions and the following	disclaimer in the documentation	and/or other materials 
// provided	with the distribution. 
// (3) Neither the name	of the newtelligence AG	nor	the	names of its contributors may be used 
// to endorse or promote products derived from this	software without specific prior	
// written permission.
//		
// THIS	SOFTWARE IS	PROVIDED BY	THE	COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES	OF MERCHANTABILITY 
// AND FITNESS FOR A PARTICULAR	PURPOSE	ARE	DISCLAIMED.	IN NO EVENT	SHALL THE COPYRIGHT	OWNER OR 
// CONTRIBUTORS	BE LIABLE FOR ANY DIRECT, INDIRECT,	INCIDENTAL,	SPECIAL, EXEMPLARY,	OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;	LOSS OF	USE, 
// DATA, OR	PROFITS; OR	BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY	OF LIABILITY, WHETHER 
// IN CONTRACT,	STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE	OR OTHERWISE) ARISING IN ANY WAY OUT 
// OF THE USE OF THIS SOFTWARE,	EVEN IF	ADVISED	OF THE POSSIBILITY OF SUCH DAMAGE.
// -------------------------------------------------------------------------
//
// Original	BlogX source code (c) 2003 by Chris	Anderson (http://simplegeek.com)
// 
// newtelligence is	a registered trademark of newtelligence	Aktiengesellschaft.
// 
// For portions	of this	software, the some additional copyright	notices	may	apply 
// which can either	be found in	the	license.txt	file included in the source	distribution
// or following	this notice. 
//
*/
#endregion

//=-------
// Copyright 2003, Scott Hanselman
// 
//	 Provided as is, with no warrenty, etc.
//	 License is	granted	to use,	copy, modify, 
//	 with or without credit	to me, just	don't
//	 blame me if it	doesn't	work.
//=-------
namespace DasBlog.Import.Radio
{
	using System;
	using System.Xml;
	using System.IO;
	using System.Text;
	using System.Collections;
	using System.Web;
	using System.Net;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using newtelligence.DasBlog.Runtime;


	public class CommentImporter
	{
		public const string DefaultCommentServer = "http://radiocomments.userland.com";

		public static int Import(string userId, string contentDirectory, string commentServer)
		{
			if(commentServer == null || commentServer.Length == 0)
			{
				// Set default comment server
				commentServer = DefaultCommentServer;
			}

			if (commentServer == null || userId == null || contentDirectory == null ||
				commentServer.Length == 0 || userId.Length	== 0 || contentDirectory.Length == 0)
			{
				throw new ArgumentException("commentServer, userId and contentDirectory are required.");
			}

            //This  tool assumes that you have already imported your radio data 
            //  Those imported posts have a "non-guid" entryid
            //   We'll enumerate those posts and check the radio comment service 
            //   collecting (scraping) comments and injecting them into AllComments.xml in your
            //   dasBlog content directory.	

			ArrayList entriesWithCommentsToFetch = new ArrayList();
			Console.WriteLine("Importing entries...");
            IBlogDataService dataService = BlogDataServiceFactory.GetService(contentDirectory,null);
			EntryCollection entries =	dataService.GetEntriesForDay(DateTime.MaxValue.AddDays(-2),TimeZone.CurrentTimeZone,String.Empty,int.MaxValue,int.MaxValue,String.Empty);
			foreach(Entry e in	entries)
			{
				//Since	Radio Entries are numbers,	NOT	Guids, we'll try to	Parse them 
				//	as longs, and if it	fails, it's	arguably a Guid.
				try
				{
					long.Parse(e.EntryId);
					Console.WriteLine(String.Format("Found  Imported Radio Entry: {0}", e.EntryId));
					entriesWithCommentsToFetch.Add(e.EntryId);
				}
				catch{}
			}


			foreach(string	entryId in	entriesWithCommentsToFetch)
			{
				string commentHtml = FetchRadioCommentHtml(commentServer, userId, entryId);

				if (commentHtml.IndexOf("No comments found.") == -1)
				{
					Regex commentRegex = new Regex(@"class=""comment"">(?<comment>\r\n(.*[^<]))",RegexOptions.Multiline|RegexOptions.IgnoreCase|RegexOptions.Compiled);
					Regex datesRegex = new Regex(@"(?<month>\d+)/(?<day>\d+)/(?<year>\d+); (?<hour>\d+):(?<min>\d+):(?<sec>\d+) (?<meridian>[A|P]M)</div>",RegexOptions.Multiline|RegexOptions.IgnoreCase|RegexOptions.Compiled);
					Regex namesRegex = new Regex(@"<div class=""date"">(?<name>.*)( &#0149;)",RegexOptions.Multiline|RegexOptions.IgnoreCase|RegexOptions.Compiled);

					MatchCollection commentMatches = commentRegex.Matches(commentHtml);
					MatchCollection datesMatches = datesRegex.Matches(commentHtml);
					MatchCollection namesMatches = namesRegex.Matches(commentHtml);
					if (commentMatches.Count != datesMatches.Count || datesMatches.Count != namesMatches.Count) 
						continue;

					//Now we've got "n" parallel arrays...

					//"For each comment we've found"
					for(int i = 0; i < commentMatches.Count;i++)
					{
						//Get the raw data
						string content = commentMatches[i].Groups["comment"].Value;
						string unparsedDate = datesMatches[i].Value;
						string name = namesMatches[i].Groups["name"].Value;
						string homepage = String.Empty;

						//Parse the Date...yank the end div (I'm not good at RegEx)
						int divLoc = unparsedDate.IndexOf("</div>");
						if (divLoc != -1) {unparsedDate = unparsedDate.Remove(divLoc,6);	}
						DateTime date = DateTime.ParseExact(unparsedDate,@"M/d/yy; h:mm:ss tt",CultureInfo.InvariantCulture);
					
						//Their captured name may be surrounded in an href...
						// the href is their homepage
					    int hrefLoc = name.IndexOf(@"""");
						if (hrefLoc != -1)
						{
							//Get their HomePage URL
							int hrefLen = name.LastIndexOf(@"""")-hrefLoc;
						    homepage = name.Substring(hrefLoc+1,hrefLen-1);

							//Get their name
							int nameLoc = name.IndexOf(@">");
							if (nameLoc != -1)
							{
								int nameLen = name.LastIndexOf(@"</")-nameLoc;
								name = name.Substring(nameLoc+1,nameLen-1);
							}
                        
						}
						//else it's just the name, so leave "name" as-is

						Comment comment = new Comment();
						comment.Content = content.Trim();
						comment.Author = name;
						//comment.EntryId = entryId;
						comment.TargetEntryId = entryId;
						comment.AuthorHomepage = homepage;
						comment.AuthorEmail = String.Empty;
						comment.CreatedLocalTime = date;
						comment.CreatedUtc = date.ToUniversalTime();

						Console.WriteLine(String.Format("Fetched	comment {0} from Radio:",comment.EntryId));
						Console.WriteLine(String.Format("  Author:   {0}",comment.Author));
						Console.WriteLine(String.Format("  Site:	 {0}",comment.AuthorHomepage));
						Console.WriteLine(String.Format("  Date:	 {0}",comment.CreatedLocalTime));
						Console.WriteLine(String.Format("  Content:  {0}",comment.Content));
						dataService.AddComment(comment);
					}
				}
				else
				{
					Console.WriteLine(String.Format("No comments for Radio Post {0}",entryId));
				}
			}
			return 0;
		}

		private	static	string FetchRadioCommentHtml(string commentServer, string userId, string postId)
		{  
			return ReadHtmlPage(GetcommentServerUrl(commentServer, userId, postId));
		}

		private	static	string GetcommentServerUrl(string commentServer, string userId, string postId)
		{
			//We'll	use	this over and over	again, just	adding the post	ID "p="
			string commentUrlString =	string.Format(@"{0}/comments?u={1}&p={2}",commentServer,userId,postId);
			return commentUrlString;
		}

		private	static	string ReadHtmlPage(string url)
		{
			String result;
			WebResponse objResponse;
			WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
			objResponse = objRequest.GetResponse();

			//Optional	International Fix:
			//Dim sr As	New	StreamReader(objResponse.GetResponseStream(), System.Text.Encoding.UTF8)
			using (StreamReader sr = 
					   new	StreamReader(objResponse.GetResponseStream()) )
			{
				result = sr.ReadToEnd();
				sr.Close();
			}
			return	result;
		}	
	}
}
