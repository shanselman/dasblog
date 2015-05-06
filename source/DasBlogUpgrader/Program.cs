using System;
using System.Collections;
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
using newtelligence.DasBlog.Web.Core;

namespace DasBlogUpgrader
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	internal class Program
	{
		private static string ContentPath;
		private static StreamWriter Log;

		public static void WriteLine(string line)
		{
			Log.WriteLine(line);
			Console.WriteLine(line);
		}

		public static void Save(DayExtra dayExtra, IBlogDataService dataService)
		{
			FileStream fileStream = FileUtils.OpenForWrite(Path.Combine(ContentPath, dayExtra.FileName));

			if (fileStream != null)
			{
				try
				{
					XmlSerializer ser = new XmlSerializer(typeof (DayExtra), "urn:newtelligence-com:dasblog:runtime:data");
					using (StreamWriter writer = new StreamWriter(fileStream))
					{
						ser.Serialize(writer, dayExtra);
					}
				}
				catch (Exception ex)
				{
					WriteLine(String.Format("ERROR: Cannot write file: {0}", dayExtra.FileName));
					WriteLine(ex.ToString());
				}
				finally
				{
					fileStream.Close();
				}
			}
		}

		public static void RepairComments(DayExtra dayExtra, IBlogDataService dataService)
		{
			//SDH: Corruption or poorly imported comments can have no entry id! 
			// Create one if it's missing. This will slowly repair the damage
			for (int i = 0; i < dayExtra.Comments.Count; i++)
			{
				if (dayExtra.Comments[i].EntryId == null)
				{
					dayExtra.Comments[i].EntryId = Guid.NewGuid().ToString();

					Entry entry = dataService.GetEntry(dayExtra.Comments[i].TargetEntryId);
					if (entry != null)
					{
						dayExtra.Comments[i].TargetTitle = entry.Title;
					}

					WriteLine(String.Format("...Repaired Comments in {0}", dayExtra.FileName));
				}
			}
		}

		public static void FixIsPublic(string path)
		{
			ContentPath = path;

			BlogDataServiceFactory.RemoveService(path);
			IBlogDataService dataService = BlogDataServiceFactory.GetService(ContentPath, null);
			EntryCollection entries = dataService.GetEntriesForDay(
				DateTime.MaxValue.AddDays(-2),
				TimeZone.CurrentTimeZone,
				String.Empty,
				int.MaxValue,
				int.MaxValue,
				String.Empty);

			foreach (Entry e in entries)
			{
				//if (e.IsPublic == false)
				{
					try
					{
						Entry edit = dataService.GetEntryForEdit(e.EntryId);
						edit.IsPublic = true;

                        if (edit.Categories == String.Empty)
                        {
                            edit.Categories = "Main";
                        }
						EntrySaveState saved = dataService.SaveEntry(edit);
					
						if (saved == EntrySaveState.Failed)
							WriteLine(String.Format("Failed saving {0}", e.Title));
						else
							WriteLine(String.Format("Saved {0}", e.Title));
					}
					catch (Exception e1)
					{
						WriteLine(String.Format("Failed saving {0}, {1}", e.Title, e1.ToString()));
					}
				}
			}
		}

		public static void FixDays(string path)
		{
			ContentPath = path;

			IBlogDataService dataService = BlogDataServiceFactory.GetService(ContentPath, null);
			
			EntryCollection entries = dataService.GetEntriesForDay(
				DateTime.MaxValue.AddDays(-2),
				TimeZone.CurrentTimeZone,
				String.Empty,
				int.MaxValue,
				int.MaxValue,
				String.Empty);

			Hashtable DayEntries = new Hashtable();

			foreach (Entry entry in entries)
			{	
				DayEntry dayEntry = new DayEntry();
				dayEntry.DateUtc = entry.CreatedUtc;

				if (DayEntries.ContainsKey(entry.CreatedUtc.Date))
					dayEntry = DayEntries[entry.CreatedUtc.Date] as DayEntry;
				dayEntry.Entries.Add(entry);
				DayEntries[entry.CreatedUtc.Date] = dayEntry;
			}

			DirectoryInfo directoryInfo = new DirectoryInfo(path);

			foreach(FileInfo fileInfo in directoryInfo.GetFiles("*.dayentry.xml"))
			{
				// backup the old file
				try
				{
					DirectoryInfo backup = new DirectoryInfo(Path.Combine(directoryInfo.FullName, "backup"));

					if (!backup.Exists)
					{
						backup.Create();
					}
						
					fileInfo.MoveTo(Path.Combine(backup.FullName, fileInfo.Name));
				}
				catch (Exception e)
				{
					ErrorTrace.Trace(TraceLevel.Error, e);
				}
			}

			foreach (DayEntry dayEntry in DayEntries.Values)
			{
				Save(dayEntry, path);
			}
		}

		static internal void Save(DayEntry dayEntry, string fullPath)
		{
			fullPath = Path.Combine(fullPath, dayEntry.FileName);

			// We use the internal list to circumvent ignoring 
			// items where IsPublic is set to false.
			if ( dayEntry.Entries.Count == 0 )
			{
				if ( File.Exists( fullPath ) )
				{
					File.Delete( fullPath );
				}
			}
			else
			{
				FileStream fileStream = FileUtils.OpenForWrite(fullPath);

				if ( fileStream != null )
				{
					try
					{
						XmlSerializer ser = new XmlSerializer(typeof(DayEntry),"urn:newtelligence-com:dasblog:runtime:data");
						using (StreamWriter writer = new StreamWriter(fileStream))
						{
							ser.Serialize(writer, dayEntry);
						}

						WriteLine(String.Format("Saved {0}", dayEntry.FileName));
					}
					catch(Exception e)
					{
						ErrorTrace.Trace(System.Diagnostics.TraceLevel.Error,e);
					}
					finally
					{
						fileStream.Close();
					}
				}
			}
		}

		public static void Upgrade(string path)
		{
			// more aggresive way to upgrade files
			foreach (FileInfo fi in new DirectoryInfo(path).GetFiles("*.dayextra.xml"))
			{
				DayExtraOld dayExtra = new DayExtraOld(fi.FullName);
				string filename = dayExtra.DateLocalTime.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".dayfeedback.xml";
				FileStream fileStream = FileUtils.OpenForWrite(Path.Combine(path, filename));
				if (fileStream != null)
				{
					try
					{
						XmlSerializer ser = new XmlSerializer(typeof (DayExtraOld), "urn:newtelligence-com:dasblog:runtime:data");
						using (StreamWriter writer = new StreamWriter(fileStream))
						{
							ser.Serialize(writer, dayExtra);
						}

						try
						{
							WriteLine(String.Format("Saved {0}", fi.Name));

							DirectoryInfo backup = new DirectoryInfo(Path.Combine(path, "backup"));

							if (!backup.Exists)
							{
								backup.Create();
							}

							fi.MoveTo(Path.Combine(backup.FullName, fi.Name));
						}
						catch (Exception ex)
						{
							WriteLine(String.Format("ERROR: Cannot delete file: {0}", fi.FullName));
							WriteLine(ex.ToString());
						}
					}
					catch (Exception e)
					{
						ErrorTrace.Trace(TraceLevel.Error, e);
						// truncate the file if this fails
						fileStream.SetLength(0);
					}
					finally
					{
						fileStream.Close();
					}
				}
			}
		}

		public static bool IsBadNode(string nodeValue)
		{
			if(nodeValue == null || nodeValue.Length == 0) return false;

			bool retVal = false;
			Match match = null;
			
			Uri UrlReferrer = null;
			try
			{
				UrlReferrer = new Uri(nodeValue);
			}
			catch(Exception)
			{
				return true; //badly formed URIs hurt folks.
			}

			// exclude the query from the referrer so we don't remove referrals from search engines
			string strippedReferrer = (UrlReferrer != null) ? UrlReferrer.Scheme + "://" + UrlReferrer.Authority + UrlReferrer.AbsolutePath : String.Empty;
			string referrer = UrlReferrer.AbsoluteUri;
            
			foreach (IBlackList referralBlacklist in ReferralBlackListFactory.Lists)
			{
				try
				{
					match = referralBlacklist.IsBlacklisted(strippedReferrer);
					if (match!= null)
					{
						retVal = match.Success;
						if (retVal) break;
					}
				}
				catch (Exception ex)
				{
					WriteLine(ex.ToString());
				}
			}
			
			if (retVal)
			{
				WriteLine(String.Format("...Found {0} in {1}, removing",match.Value,referrer));
				return true;
			}
			
			return false;
		}
		public static void FixReferralSpam (string path)
		{
			string bad = ConfigurationManager.AppSettings["badWords"];
			long badWordsCount = 0;
			long totalBadWordsCount = 0;
			
			DirectoryInfo info = new DirectoryInfo("../../");

			ReferralBlackListFactory.AddBlacklist(new MovableTypeBlacklist(), Path.Combine(info.FullName, "blacklist.txt"));
			
			if (bad != null && bad.Length > 0) 
				ReferralBlackListFactory.AddBlacklist(new ReferralUrlBlacklist(), bad);
			
			string ourNamespace = "urn:newtelligence-com:dasblog:runtime:data";

			foreach (string file in Directory.GetFiles(path,"*.dayfeedback.xml"))
			{
				badWordsCount = 0;
				NameTable nt = new NameTable();
				object permaLink = nt.Add("PermaLink");
				XmlNamespaceManager ns = new XmlNamespaceManager(nt);
				ns.AddNamespace("def", ourNamespace);
				XmlDocument x = new XmlDocument(nt);
				try
				{
					x.Load(file);
				}
				catch (XmlException ex)
				{
					WriteLine(String.Format("ERROR: Malformed Xml in file: {0}",file));
					WriteLine(ex.ToString());
					Console.WriteLine("Press ENTER to continue...");
					Console.ReadLine();
				}

				XmlNodeList nodes = x.SelectNodes("/def:DayExtra/def:Trackings/def:Tracking", ns);
				Console.WriteLine("Found {0} trackings/referrals in {1}",nodes.Count,file);
				for (int i = 0; i < nodes.Count; i++)
				{
					XmlNode node = nodes[i];
				XmlNode permaLinkNode = node[(string)permaLink];
					if (permaLinkNode != null && IsBadNode(permaLinkNode.InnerText))
					{
						badWordsCount++;
						totalBadWordsCount++;
						node.ParentNode.RemoveChild(node);
					}
				}
				if (badWordsCount > 0)
				{
					x.Save(file);
				}
				WriteLine(String.Format("Found {0} bad words in {1}...", badWordsCount, Path.GetFileName(file)));
			}
		}


		public static void YankReferrals (string path)
		{
			string ourNamespace = "urn:newtelligence-com:dasblog:runtime:data";
			foreach (string file in Directory.GetFiles(path,"*.dayfeedback.xml"))
			{
				NameTable nt = new NameTable();
				object permaLink = nt.Add("PermaLink");
				XmlNamespaceManager ns = new XmlNamespaceManager(nt);
				ns.AddNamespace("def", ourNamespace);
				XmlDocument x = new XmlDocument(nt);
				try
				{
					x.Load(file);
				}
				catch (XmlException ex)
				{
					WriteLine(String.Format("ERROR: Malformed Xml in file: {0}",file));
					WriteLine(ex.ToString());
					Console.WriteLine("Press ENTER to continue...");
					Console.ReadLine();
				}

				XmlNodeList nodes = x.SelectNodes("/def:DayExtra/def:Trackings/def:Tracking[def:TrackingType = \"Referral\"]", ns);
				for (int i = 0; i < nodes.Count; i++)
				{
					XmlNode node = nodes[i];
					node.ParentNode.RemoveChild(node);
				}
				if (nodes.Count > 0)
				{
					x.Save(file);
				}
				WriteLine(String.Format("Removed {0} referrals in {1}",nodes.Count,file));
			}
		}


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Console.WriteLine("BACKUP YOUR CONTENT FOLDER BEFORE RUNNING THIS!");

			// we need to set the role to admin so we get public entries
			System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(System.Threading.Thread.CurrentPrincipal.Identity, new string[] {"admin"});

			if (args.Length != 1)
			{
				Console.WriteLine(@"Usage: DasBlogUpgrader ""c:\mydasblog\content""");
				Environment.Exit(1);
				return;
			}
			Console.WriteLine("Press ENTER to continue...");
			Console.ReadLine();

			string path = args[0];
			Log = new StreamWriter(Path.Combine(path, "upgradeLog.txt"), false);
			if (Directory.Exists(path))
			{
				Console.WriteLine("Would you like to fix the days your entries are in (recomended) (y,n)?");
				string ans = Console.ReadLine();

				if (ans.ToLower() == "y")
				{
					FixDays(path);
				}

				if (new DirectoryInfo(path).GetFiles("*.dayextra.xml").Length > 0)
					Upgrade(path);
				
				WriteLine("Upgrade of dayExtra complete");
			}
			else
			{
				WriteLine("No dayExtra files to upgrade");
			}

			Console.WriteLine("Would you like to make sure all your entries are set to Public (y,n)?");
			string answer = Console.ReadLine();

			if (answer.ToLower() == "y")
			{
				Console.WriteLine("");
				FixIsPublic(path);
			}


			Console.WriteLine("");
			Console.WriteLine("If you run a high-traffic site, we recommend you turn off storage of referrals as they bloat your dayfeedback files. They are still stored in logs.\nWuld you like to remove referrals from your dayfeedback files?(y,n)?");
			answer = Console.ReadLine();

			if (answer.ToLower() == "y")
			{
				YankReferrals(path);		
			}



			Console.WriteLine("");
			Console.WriteLine("Would you like to clean your files of Spam (y,n)?");
			answer = Console.ReadLine();

			if (answer.ToLower() == "y")
			{
				FixReferralSpam(path);		
			}

			try
			{
				if (File.Exists(Path.Combine(path, "entryCache.xml")))
					File.Delete(Path.Combine(path, "entryCache.xml"));

				if (File.Exists(Path.Combine(path, "categoryCache.xml")))
					File.Delete(Path.Combine(path, "categoryCache.xml"));

				if (File.Exists(Path.Combine(path, "blogdata.xml")))
					File.Delete(Path.Combine(path, "blogdata.xml"));

				if (File.Exists(Path.Combine(path, "AllComments.xml")))
					File.Delete(Path.Combine(path, "AllComments.xml"));
			}
			catch (Exception e)
			{
				WriteLine(e.ToString());
			}

			//Check all the files again for bad xml which can happen during multithreaded FTP downloads with SmartFTP,
			// plus it doesn't hurt and can catch corruption you don't know you have!
			foreach (string file in Directory.GetFiles(path,"*.xml"))
			{
				try
				{
					XmlDocument x = new XmlDocument();
					x.Load(file);
				}
				catch (XmlException ex)
				{
					Console.WriteLine("ERROR: Malformed Xml in file: {0}",file);
					Console.WriteLine(ex.ToString());
				}
			}

			WriteLine(String.Format("Upgrade of conent is complete. A log file has been saved in {0}", Path.Combine(path, "upgradeLog.txt")));
			Log.Close();
			Console.ReadLine();
		}
	}
}
