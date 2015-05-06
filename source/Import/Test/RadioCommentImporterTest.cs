using DasBlog.Import.Radio;
using DasBlog.Import.Test;
using newtelligence.DasBlog.Runtime;
using newtelligence.DasBlog.Util;
using NUnit.Framework;
using System;
using System.IO;


namespace DasBlog.Import.Radio.Test
{
	[TestFixture]
	public class CommentImporterTest : ImporterBaseTest
	{

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			EntryImporter.Import(EntryImporterTest.SourceDirectory, 
				ContentDirectory);
		}

		public string UserID
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get 
			{ 
				return _userID; 
			}  
			[System.Diagnostics.DebuggerStepThrough()]
			set
			{ 
				_userID = value; 
			}  
		} 
		// The userId below belongs to Mark Michaelis (mark@michaelis.net).  Please use
		// it with caution.  Thanks!
		private string _userID = "114349";

		[Test, Ignore("Code not yet completed")]
		public void Import()
		{

			CommentImporter.Import(UserID, ContentDirectory, null);

			CommentCollection comments = DataService.GetAllComments();
			Assert.AreEqual(15, comments.Count);
		}

		#region Check Invalid Parameters
		[Test, ExpectedException(typeof(ArgumentException))]
		public void UserIdIsNull()
		{
			CommentImporter.Import(null, "garbage", null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void UserIdIsEmpty()
		{
			CommentImporter.Import("", "garbage", null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ContentDirectoryIsNull()
		{
			CommentImporter.Import("garbage", null, null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ContentDirectoryIsEmpty()
		{
			CommentImporter.Import("garbage", "", null);
		}
		#endregion Check Invalid Parameters

	}
}
