#region Copyright (c) 2003, newtelligence AG. All rights reserved.
/*
// Copyright (c) 2003, newtelligence AG. (http://www.newtelligence.com)
// Original BlogX Source Code: Copyright (c) 2003, Chris Anderson (http://simplegeek.com)
// All rights reserved.
//  
// Redistribution and use in source and binary forms, with or without modification, are permitted 
// provided that the following conditions are met: 
//  
// (1) Redistributions of source code must retain the above copyright notice, this list of 
// conditions and the following disclaimer. 
// (2) Redistributions in binary form must reproduce the above copyright notice, this list of 
// conditions and the following disclaimer in the documentation and/or other materials 
// provided with the distribution. 
// (3) Neither the name of the newtelligence AG nor the names of its contributors may be used 
// to endorse or promote products derived from this software without specific prior 
// written permission.
//      
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// -------------------------------------------------------------------------
//
// Original BlogX source code (c) 2003 by Chris Anderson (http://simplegeek.com)
// 
// newtelligence is a registered trademark of newtelligence Aktiengesellschaft.
// 
// For portions of this software, the some additional copyright notices may apply 
// which can either be found in the license.txt file included in the source distribution
// or following this notice. 
//
*/
#endregion

//=-------
// Copyright 2003, Torsten Rendelmann
// 
//   Provided as is, with no warrenty, etc.
//   License is granted to use, copy, modify, 
//   with or without credit to me, just don't
//   blame me if it doesn't work.
//=-------
namespace Rendelmann.Torsten.DasBlog.BlogWorksXmlImport
{
    using System;
    using System.Xml;
	using System.Xml.XPath;
    using System.IO;
    using System.Text;
    using System.Collections;
	using System.Collections.Specialized;
	using newtelligence.DasBlog.Runtime;

    class App
    {
        static string FileSystemResolver(string file)
        {
            return Path.Combine(to, file);
        }

        static string from;
        static string to;
		static string id = "001";

        static int Main(string[] args)
        {
            Console.WriteLine("BlogWorksXML Importer");
            Console.WriteLine("(import supports BlogWorks version 1.1 and above)");
            

            foreach (string arg in args)
            {
                if (arg.Length > 6 && arg.ToLower().StartsWith("/from:"))
                {
                    from = arg.Substring(6).Trim();
                    if (from[0] == '\"' && from[from.Length] == '\"')
                    {
                        from = from.Substring(1, from.Length - 2);
                    }
                }
				else if (arg.Length > 6 && arg.ToLower().StartsWith("/id:")) {
					id = arg.Substring(4).Trim();
				}
				else if (arg.Length > 6 && arg.ToLower().StartsWith("/to:"))
                {
                    to = arg.Substring(4).Trim();
                    if (to[0] == '\"' && to[from.Length] == '\"')
                    {
                        to = to.Substring(1, to.Length - 2);
                    }
                }
                else
                {
                    break;
                }
            }

            if (from == null || to == null || id == null || from.Length == 0 || to.Length == 0 || id.Length == 0)
            {
                Console.WriteLine("Usage: impbwxml /from:<blogworks data directory> [/id:<blogworks blog id, e.g. 001>] /to:<output directory>");
                Console.WriteLine("");
                return -1;
            }
            

            IBlogDataService dataService = BlogDataServiceFactory.GetService(to,null);
            

            Console.WriteLine("Importing entries from...");

            ArrayList tables = new ArrayList();
			ArrayList comments = new ArrayList();
			Hashtable commentRefs = new Hashtable();

            XmlDocument masterDoc = new XmlDocument();
            StringBuilder sb = new StringBuilder();
            sb.Append("<tables>");

            foreach (FileInfo file in new DirectoryInfo(from).GetFiles("*archive"+id+".xml"))
            {
				Console.Write("  * " + file.Name);
				XmlDocument doc = new XmlDocument();
				doc.Load(file.FullName);
				foreach (XmlNode n in doc.SelectNodes("/baef/blog")) {
					sb.Append(n.OuterXml);
				}
				Console.WriteLine(" ... done.");
			}
            sb.Append("</tables>");

            masterDoc.Load(new StringReader(sb.ToString()));

            foreach (XmlNode node in masterDoc.FirstChild)
            {
                BlogWorksTable table = new BlogWorksTable();
                table.Name = node.Attributes["id"].Value;

                foreach (XmlNode child in node)	//  author with authorname, authormail childs
                {
                    switch (child.Name) {
						case "author":
							break;	// ignore. dasBlog is not yet multiuser enabled
						case "information":
							foreach (XmlNode infoNode in child) {	//  commentthread; timestamp; language; categories
								// how about commentthread ?
								switch (infoNode.Name) {
									case "commentthread":
										// save the reference for later use
										commentRefs.Add(infoNode.InnerText, table.Name);
										break;
									case "timestamp":
										table.Data[infoNode.Name] = UnixToHuman(infoNode.InnerText);
										break;
									case "language":
										if (infoNode.InnerText != "en")
											table.Data[infoNode.Name] = infoNode.InnerText;
										break;
									case "categories":
										foreach (XmlNode catNode in infoNode) {
											if (catNode.InnerText.Length > 0) {
												if (table.Data.Contains("categories")) {
													table.Data["categories"] = (string)table.Data["categories"] + ";" + catNode.InnerText;
												}
												else {
													table.Data["categories"] = catNode.InnerText;
												}
											}
										}
										break;
								}
							}
							if (!table.Data.Contains("categories")) {
								table.Data["categories"] = "General";
							}
							break;

						case "text":	// blogtitle (entry title); blogbody (entry body)
							foreach (XmlNode textNode in child) {
								switch (textNode.Name) {
									case "blogtitle":
										table.Data[textNode.Name] = textNode.InnerText;
										break;
									case "blogbody":
										table.Data[textNode.Name] = textNode.InnerText;
										break;
								}
							}
							break;
                    }
                }
                tables.Add(table);
            }

			Console.WriteLine("Now writing entries....");
			
			foreach (BlogWorksTable table in tables)
            {
                Entry entry = new Entry();
                entry.CreatedUtc = table.When;
                entry.Title = table.Title;
                entry.Content = table.Text;
                entry.Categories = table.Categories;
                entry.EntryId = table.UniqueId;
				entry.Language = table.Language;
                dataService.SaveEntry( entry );
            }
			

			Console.WriteLine("Finished. Start reading comments...");


			masterDoc = new XmlDocument();
			sb = new StringBuilder();
			sb.Append("<comments>");

			foreach (FileInfo file in new DirectoryInfo(from).GetFiles("*comment"+id+".xml")) {
				Console.Write("  * " + file.Name);
				XmlDocument doc = new XmlDocument();
				doc.Load(file.FullName);
				foreach (XmlNode n in doc.SelectNodes("/comments/thread")) {
					sb.Append(n.OuterXml);
				}
				Console.WriteLine(" ... done.");
			}
			sb.Append("</comments>");

			masterDoc.Load(new StringReader(sb.ToString()));

			foreach (XmlNode node in masterDoc.FirstChild) {

				string threadId = node.Attributes["id"].Value;
				
				if (!commentRefs.Contains(threadId))
					continue;

				foreach (XmlNode cmtNode in node) {	//  comment's per thread

					BlogWorksComment comment = new BlogWorksComment();
					comment.Name = (string)commentRefs[threadId];	// get corresponding entry Id

					foreach (XmlNode child in cmtNode) {	//  comment elements
						
						switch (child.Name) {
							case "name":
								comment.Data[child.Name] = child.InnerText;	// Author
								break;
							case "datetime":
								comment.Data[child.Name] = DateTime.Parse(child.InnerText);
								break;
							case "email":
								comment.Data[child.Name] = child.InnerText;	
								break;
							case "uri":	
								if (child.InnerText.Length > 7 /* "http://".Length */)
									comment.Data[child.Name] = child.InnerText;	
								break;
							case "text":	
								comment.Data[child.Name] = child.InnerText;	
								break;
							case "ip":	
								comment.Data[child.Name] = child.Clone();	// anyElement
								break;
						}
					}//child
					comments.Add(comment);
				}//cmtNode

			}

			Console.WriteLine("Now writing comment entries....");
			
			foreach (BlogWorksComment cmt in comments) {
				Comment comment = new Comment();
				comment.Content = cmt.Text;
				comment.Author = cmt.Author;
				comment.TargetEntryId = cmt.UniqueId;
				comment.AuthorHomepage = cmt.AuthorHomepage;
				comment.AuthorEmail = cmt.AuthorEmail;
				comment.CreatedLocalTime = cmt.When;
				comment.CreatedUtc = cmt.When.ToUniversalTime();
				comment.anyElements = new XmlElement[]{cmt.Ip};

				dataService.AddComment(comment);
			}
			
			Console.WriteLine("Finished. Start reading comments...");

			Console.WriteLine("Finished successfully.");
			return 0;
        }

		static DateTime UnixToHuman(string timestamp) {
			if (timestamp == null || timestamp == String .Empty) {
				return DateTime.Now;
			}
			DateTime target_date = new  DateTime(1970, 1, 1);
			return target_date.AddSeconds(Double.Parse(timestamp));
		}
    }

    class BlogWorksTable:IComparable
    {
        Hashtable data = new Hashtable();
        string name;

        public BlogWorksTable() {}

        public string Name { get { return name; } set { name = value; } }
        public string UniqueId { get { return name; } }
        public IDictionary Data { get { return data; } }
        public DateTime When { get { return (DateTime)Data["timestamp"]; } }
        public string Text { get { return (string)Data["blogbody"]; } }
        public string Title { get { return (string)Data["blogtitle"]; } }
		public string Language { get { return (string)Data["language"]; } }
		public string Categories { get { return (string)Data["categories"]; } }
		#region IComparable Members

		public int CompareTo(object obj) {
			BlogWorksTable other = obj as BlogWorksTable;
			if (other != null)
				return this.When.CompareTo(other.When);
			else
				return 0;
		}

		#endregion
	}

	class BlogWorksComment
	{
		Hashtable data = new Hashtable();
		string name;

		public BlogWorksComment() {}

		public string Name { get { return name; } set { name = value; } }
		public string UniqueId { get { return name; } }
		public IDictionary Data { get { return data; } }
		public DateTime When { get { return (DateTime)Data["datetime"]; } }
		public string Text { get { return (string)Data["text"]; } }
		public string AuthorEmail { get { return (string)Data["email"]; } }
		public string Author { get { return (string)Data["name"]; } }
		public string AuthorHomepage { get { return (string)Data["uri"]; } }
		public XmlElement Ip { get { return (XmlElement)Data["ip"]; } }
	}
}

