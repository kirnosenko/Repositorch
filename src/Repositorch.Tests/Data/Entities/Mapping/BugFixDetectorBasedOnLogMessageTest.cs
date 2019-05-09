using System;
using Xunit;

namespace Repositorch.Data.Entities.Mapping
{
	public class BugFixDetectorBasedOnLogMessageTest
	{
		private BugFixDetectorBasedOnLogMessage detector;
		private Commit commit;

		public BugFixDetectorBasedOnLogMessageTest()
		{
			detector = new BugFixDetectorBasedOnLogMessage();
			commit = new Commit();
		}
		[Fact]
		public void Can_detect_commit_with_word_fix_in_message_as_bugfix()
		{
			var testCases = new (string message, bool result)[]
			{
				("simple log message", false),
				("some fixtures were added", false),
				("new string prefix", false),
				("fix for some feature", true),
				("this is fix too", true),
				("it is bugfix", true),
				("bug was fixed", true),
				("One More Fix.", true),
			};
			foreach (var testCase in testCases)
			{
				commit.Message = testCase.message;
				Assert.Equal(testCase.result, detector.IsBugFix(commit));
			}
		}
		[Fact]
		public void Can_detect_bugfix_for_winmerge()
		{
			var testCases = new (string message, bool result)[]
			{
				("Fixed warnings", false),
				("PATCH: [ 715553 ] Add commandline options to not add paths to MRU", false),
				("Apply icon change (2004-03-13 Seier) to languages", false),
				("Text modifications", false),
				("Fix 'cannot select CDROM directory' problem", true),
				("bug fix scroll to first diff.", true),
				("fix some bugs with undo and merge", true),
				("bugfix to use system colours for unselected lines in directory.", true),
				("Bug #125572: state of recurse checkbox is now saved", true),
				("[ 686699 ] Check &amp; return file saving success - fix", true),
				("BUG: [ 683753 ] Rescan is not prompting to save dirty current file", true),
				("PATCH: [ 709502 ] Fix missing/existing EOL at end of file", true),
			};
			foreach (var testCase in testCases)
			{
				commit.Message = testCase.message;
				Assert.Equal(testCase.result, detector.IsBugFix(commit));
			}
		}
		[Fact]
		public void Can_be_configured_with_keywords_and_stopwords()
		{
			detector.KeyWords = new string[] { "bug", "error" };
			detector.StopWords = new string[] { "not", "no" };

			commit.Message = "this is error";
			Assert.True(detector.IsBugFix(commit));

			commit.Message = "this is not error";
			Assert.False(detector.IsBugFix(commit));
		}
	}
}
