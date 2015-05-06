using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using newtelligence.DasBlog.Runtime;
using System.Collections.Generic;

namespace newtelligence.DasBlog.Web.Core
{
	/// <summary>
	/// Enumeration used for the return code of <see cref="IBlogDataService.SaveEntry"/>
	/// </summary>
	public enum BlacklistUpdateState
	{
		None, 
		Updated, 
		Failed 
	}

	public interface IBlackList
	{
		void Initialize(string blacklist);
		BlacklistUpdateState UpdateBlacklist();
		Match IsBlacklisted(string url);
	}

	public class ReferralBlackListFactory
	{
		private static Hashtable blacklists = new Hashtable();

		public static void AddBlacklist(IBlackList blackList, string blacklist)
		{
			if (blacklists.ContainsKey(blackList.GetType().Name) == false)
			{
				try
				{
					blackList.Initialize(blacklist);
					blackList.UpdateBlacklist();
					blacklists.Add(blackList.GetType().Name, blackList);
				}
				catch (Exception ex)
				{
					Console.Write(ex.ToString());
				}
			}
			else
			{
				// update the blacklist
				IBlackList referrerBlacklist = blacklists[blackList.GetType().Name] as IBlackList;
				try
				{
					referrerBlacklist.Initialize(blacklist);
					BlacklistUpdateState updateState = referrerBlacklist.UpdateBlacklist();
					
					if (updateState == BlacklistUpdateState.Failed)
					{
						new EventDataItem(EventCodes.Error, blackList.ToString() + " could not be updated: ", "");
					}

				}
				catch (Exception ex)
				{
					Console.Write(ex.ToString());
				}
			}
		}

		public static void RemoveBlacklist(Type type)
		{
			if (blacklists.ContainsKey(type.Name)== true)
			{
				try
				{
					blacklists.Remove(type.Name);
				}
				catch (Exception ex)
				{
					Console.Write(ex.ToString());
				}
			}
		}


		public static IBlackList[] Lists
		{
			get
			{
				List<IBlackList> list = new List<IBlackList>();
				foreach (IBlackList referralBlacklist in blacklists.Values)
				{
					list.Add(referralBlacklist);
				}

				return list.ToArray();
			}
		}
	}
}
