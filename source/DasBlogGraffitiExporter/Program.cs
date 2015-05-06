using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using newtelligence.DasBlog.Runtime;
using newtelligence.DasBlog.Util;
using log4net;
using System.Xml.Schema;
using Graffiti.Core;

namespace DasBlogGraffitiExporter
{
    class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string InternalCompressTitle(string titleParam)
        {
            if (titleParam == null || titleParam.Length == 0)
            {
                return String.Empty;
            }

            StringBuilder retVal = new StringBuilder(titleParam.Length);

            bool upper = false;
            bool pendingSpace = false;
            bool tag = false;

            for (int i = 0; i < titleParam.Length; ++i)
            {
                char c = titleParam[i];

                if (tag)
                {
                    if (c == '>')
                    {
                        tag = false;
                    }
                }
                else if (c == '<')
                {
                    // Discard everything between '<' and '>', inclusive.
                    // If no '>', just discard the '<'.
                    tag = (titleParam.IndexOf('>', i) >= 0);
                }

                    // Per RFC 2396 (URI syntax):
                //  delims   = "<" | ">" | "#" | "%" | <">
                //  reserved = ";" | "/" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ","
                // These characters should not appear in a URL
                else if ("#%\";/?:@&=$,".IndexOf(c) >= 0)
                {
                    continue;
                }

                else if (char.IsWhiteSpace(c))
                {
                    pendingSpace = true;
                }

                    // The marks may appear in URLs
                //  mark = "-" | "_" | "." | "!" | "~" | "*" | "'" | "(" | ")"
                // as may all alphanumerics. (Tilde gets mangled by UrlEncode).
                // Here we are more lenient than RFC 2396, as we allow
                // all Unicode alphanumerics, not just the US-ASCII ones.
                // SDH: Update: it's really safer and maintains more permalinks if we stick with A-Za-z0-9.
                else if (char.IsLetterOrDigit(c) /* ||  "-_.!~*'()".IndexOf(c) >= 0 */)
                {
                    if (pendingSpace)
                    {
                        // Discard leading spaces
                        if (retVal.Length > 0)
                        {
                            // The caller will strip '+' if !siteConfig.EnableTitlePermaLinkSpaces
                            retVal.Append("-");
                        }
                        upper = true;
                        pendingSpace = false;
                    }

                    if (upper)
                    {
                        retVal.Append(char.ToUpper(c));
                        upper = false;
                    }
                    else
                    {
                        retVal.Append(c);
                    }
                }
            }

            return retVal.ToString();
        }

        private static string GetDasBlog301Folder()
        {
            string dasBlog301Folder = Path.Combine(Directory.GetCurrentDirectory(), "dasBlog301");
            DirectoryInfo dir = new DirectoryInfo(dasBlog301Folder);
            if (dir.Exists == false)
                dir.Create();
            return dasBlog301Folder;
        }

        static void Main(string[] args)
        {
            logger.Info("Starting");
            // we need to set the role to admin so we get public entries
            System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(System.Threading.Thread.CurrentPrincipal.Identity, new string[] { "admin" });

            if (args.Length != 1)
            {
                logger.Error(@"Usage: DasBlogGraffitiExporter ""c:\mydasblog\content""");
                Environment.Exit(1);
                return;
            }
            
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();

            string path = args[0];
            if (Directory.Exists(path))
            {
                // Check all the files again for bad xml which can happen during multithreaded FTP downloads with SmartFTP,
                // plus it doesn't hurt and can catch corruption you don't know you have!
                foreach (string file in Directory.GetFiles(path, "*.xml"))
                {
                    try
                    {
                        XmlDocument x = new XmlDocument();
                        x.Load(file);
                    }
                    catch (XmlException ex)
                    {
                        logger.FatalFormat("ERROR: Malformed Xml in file: {0}", file);
                        logger.Fatal("XmlException", ex);
                    }
                }

                IBlogDataService dataService = BlogDataServiceFactory.GetService(path, null);

                List<UrlMap> categoryMap = new List<UrlMap>();
                CategoryCacheEntryCollection categories = dataService.GetCategories();
                logger.InfoFormat("Found {0} Categories", categories.Count);

                foreach (CategoryCacheEntry entry in categories)
                {
                    string name = HttpHelper.GetURLSafeString(entry.Name);
                    UrlMap mapping = new UrlMap(name, InternalCompressTitle(entry.DisplayName));
                    categoryMap.Add(mapping);
                    logger.InfoFormat("Adding category name:{0}, display name:{1}", entry.Name, entry.DisplayName);
                }

                UrlMap.Save(categoryMap, Path.Combine(GetDasBlog301Folder(), "CategoryMapping.xml"));

                EntryCollection entries = dataService.GetEntriesForDay(
                    DateTime.MaxValue.AddDays(-2),
                    TimeZone.CurrentTimeZone,
                    String.Empty,
                    int.MaxValue,
                    int.MaxValue,
                    String.Empty);
                logger.InfoFormat("Found {0} entries", entries.Count);

                List<UrlMap> permalinkMapping = new List<UrlMap>();

                foreach (Entry entry in entries)
                {
                    string title = entry.CompressedTitle;
                    UrlMap mapping = new UrlMap(entry.EntryId, title);
                    permalinkMapping.Add(mapping);
                    logger.InfoFormat("Adding entry id:{0} title:{1}", entry.EntryId, entry.CompressedTitle); 
                }

                UrlMap.Save(permalinkMapping, Path.Combine(GetDasBlog301Folder(), "PermalinkMapping.xml"));
            }
        }
    }
}
