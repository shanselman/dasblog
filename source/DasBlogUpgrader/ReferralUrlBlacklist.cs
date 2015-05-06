using System;
using System.Text.RegularExpressions;

namespace newtelligence.DasBlog.Web.Core
{
	/// <summary>
	/// Summary description for ContentReferralBlacklist.
	/// </summary>
	public class ReferralUrlBlacklist : IBlackList
	{
		private static bool loaded = false;
		private static Regex blackListRegex = null;
		private static string blacklist = null;
		private static object blackListArrayLock = new object();
		private static object blackListRegexLock = new object();

		public void Initialize(string newBlackList)
		{
			lock(blackListRegexLock)
			{
				if (blacklist == null || blacklist != newBlackList)
				{
					if (newBlackList != null && newBlackList.Length > 0)
					{
						blacklist = newBlackList;
						blacklist = blacklist.Replace(";","|");
						blacklist = blacklist.Replace("+","\\+");
						blacklist = blacklist.Replace("*","\\*");
						blackListRegex = new Regex(blacklist,RegexOptions.Compiled|RegexOptions.IgnoreCase|RegexOptions.IgnorePatternWhitespace);
					}
					else
					{
						throw new NullReferenceException("blacklist string is empty");
					}
				}
			}

			loaded = true;
		}

		public Match IsBlacklisted(string url)
		{
			if (!loaded)
			{
				return null;
			}

			try
			{
				Match match = null;

				lock (blackListArrayLock)
				{
					match = blackListRegex.Match(url);
				}

				return match;
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("An error occured trying to determine if {0} is blacklisted", url), ex.InnerException);
			}
		}

		public BlacklistUpdateState UpdateBlacklist()
		{
			return BlacklistUpdateState.None;
		}
	}
}
