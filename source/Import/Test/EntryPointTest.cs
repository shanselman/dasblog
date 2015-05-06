using DasBlog.Import;
using NUnit.Framework;
using newtelligence.DasBlog.Runtime;
using newtelligence.DasBlog.Util;
using System.IO;
using System;

namespace DasBlog.Import.Test
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	[TestFixture]
	public class EntryPointTest : ImporterBaseTest
	{

		[Test, Ignore("Test depends on data that does not exist in source control.")]
		public void ImportContentFromRadio()
		{
			EntryPoint.DllMain("DummyProgramName.exe " +
				"/SourceType:Radio " +
				"/from:\"" + ReflectionHelper.CodeBase() + "Radio\\SourceDir\" " +
				"/to:\"" + ReflectionHelper.CodeBase() + "Radio\\ContentDir\" ");
			Assert.AreEqual(BlogSourceType.Radio, EntryPoint.CommandLine.Source, 
				"The commandline was not parsed as expected.");
			Assert.AreEqual(ReflectionHelper.CodeBase() + "Radio\\SourceDir", 
				EntryPoint.CommandLine.SourceDirectory, 
				"The source directory was not parsed successfully.");
			Assert.AreEqual(ReflectionHelper.CodeBase() + "Radio\\ContentDir", 
				EntryPoint.CommandLine.ContentDirectory,
				"The content directory was not parsed successfully.");

			Assert.AreEqual(30, DataService.GetEntriesForDay(DateTime.MaxValue.AddDays(-2), TimeZone.CurrentTimeZone,
				"", int.MaxValue, int.MaxValue, null).Count);	
		}

		[Test]
		public void GetHelpText()
		{
			EntryPoint.DllMain("DummyProgramName.exe /Help");
			Assert.IsTrue(EntryPoint.CommandLine.Help, "The CommandLine was unexpectedly not set to true when using the '/Help' parameters.");
		}

		[Test, Ignore("Not yet implemented and tested")]
		public void ImportCommentsFromRadio()
		{
			EntryPoint.DllMain("DummyProgramName.exe " +
				"/CommentServer:http://radiocomments.userland.com " +
				"/userid:106747 " +
				"/ContentDir:\"" + ReflectionHelper.CodeBase() + "Radio\\ContentDir\" ");
			Assert.AreEqual(BlogSourceType.Radio, EntryPoint.CommandLine.Source, 
				"The commandline was not parsed as expected.");
			Assert.AreEqual(ReflectionHelper.CodeBase() + "Radio\\SourceDir", 
				EntryPoint.CommandLine.SourceDirectory, 
				"The source directory was not parsed successfully.");
			Assert.AreEqual(ReflectionHelper.CodeBase() + "Radio\\ContentDir", 
				EntryPoint.CommandLine.ContentDirectory,
				"The content directory was not parsed successfully.");
		}

	}
}
