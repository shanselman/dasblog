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
// Copyright 2003, Chris Anderson
//
//   Provided as is, with no warrenty, etc.
//   License is granted to use, copy, modify,
//   with or without credit to me, just don't
//   blame me if it doesn't work.
//=-------
namespace DasBlog.Import.Radio
{
    using System;
    using System.Xml;
    using System.IO;
    using System.Text;
    using System.Collections;
    using newtelligence.DasBlog.Runtime;

    public class EntryImporter
    {
        public static int Import(string from, string to)
        {
			if(from == null || from.Length == 0)
			{
				throw new ArgumentException("The source directory is required.");
			}
			else if(to == null || to.Length == 0)
			{
				throw new ArgumentException("The content directory is required.");
			}
			else if(!Directory.Exists(from))
			{
				throw new ArgumentException(
					string.Format("The source directory, '{0}' does not exist.", from));
			}
			else if(!Directory.Exists(to))
			{
				throw new ArgumentException(
					string.Format("The content directory, '{0}' does not exist.", to));
			}

			IBlogDataService dataService = BlogDataServiceFactory.GetService(to,null);

            ArrayList tables = new ArrayList();

            XmlDocument masterDoc = new XmlDocument();
            StringBuilder sb = new StringBuilder();
            sb.Append("<tables>");

            foreach (FileInfo file in new DirectoryInfo(from).GetFiles("*.xml"))
            {
                XmlDocument entry = new XmlDocument();
                entry.Load(file.FullName);
                sb.Append(entry.FirstChild.NextSibling.OuterXml);
            }
            sb.Append("</tables>");

            masterDoc.Load(new StringReader(sb.ToString()));

            foreach (XmlNode node in masterDoc.FirstChild)
            {
                EntryTable table = new EntryTable();
                table.Name = node.Attributes["name"].Value;
                foreach (XmlNode child in node)
                {
                    switch (child.Name)
                    {
                        case "date":
                            table.Data[child.Attributes["name"].Value] = DateTime.Parse(child.Attributes["value"].Value);
                            break;
                        case "boolean":
                            table.Data[child.Attributes["name"].Value] = bool.Parse(child.Attributes["value"].Value);
                            break;
                        case "string":
                            table.Data[child.Attributes["name"].Value] = child.Attributes["value"].Value;
                            break;
                        case "table":
                            if (child.Attributes["name"].Value == "categories")
                            {
                                foreach (XmlNode catNode in child)
                                {
                                    if (catNode.Name == "boolean" && catNode.Attributes["value"].Value == "true")
                                    {
                                        if (table.Data.Contains("categories"))
                                        {
                                            table.Data["categories"] = (string)table.Data["categories"] + ";" + catNode.Attributes["name"].Value;
                                        }
                                        else
                                        {
                                            table.Data["categories"] = catNode.Attributes["name"].Value;
                                        }
                                    }
                                }
                            }
                            break;
                        case "link":
                            table.Data[child.Attributes["name"].Value] = child.Attributes["value"].Value;
                            break;
						case "flNotOnHomePage":
							table.Data[child.Attributes["name"].Value] = child.Attributes["value"].Value;
							break;
                    }
                }
                tables.Add(table);
            }

            foreach (EntryTable table in tables)
            {
                Entry entry = new Entry();
                entry.CreatedUtc = table.When;
                entry.Title = table.Title;
                entry.Link = table.Link;
                entry.Content = table.Text;
                entry.Categories = table.Categories;
                entry.EntryId = table.UniqueId;
				entry.ShowOnFrontPage = !table.NotOnHomePage;
                dataService.SaveEntry( entry );
            }

			Console.WriteLine("{0} entrys were imported", tables.Count);
            return 0;
        }
    }


}
