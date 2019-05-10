using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class BugFixSelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_select_bugfixes_for_commits()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2").IsBugFix()
			.Submit()
				.AddCommit("3").IsBugFix()
			.Submit();

			Assert.Equal(
				0,
				selectionDSL
					.Commits().RevisionIs("1")
					.BugFixes().InCommits().Count());
			Assert.Equal(
				1,
				selectionDSL
					.Commits().RevisionIs("2")
					.BugFixes().InCommits().Count());
			Assert.Equal(
				1,
				selectionDSL
					.Commits().RevisionIs("3")
					.BugFixes().InCommits().Count());
		}
		[Fact]
		public void Should_select_commits_are_bugfixes_or_not()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2").IsBugFix()
			.Submit()
				.AddCommit("3").IsBugFix()
			.Submit();

			Assert.Equal(
				new string[] { "2", "3" },
				selectionDSL
					.Commits().AreBugFixes()
					.Select(x => x.Revision));
			Assert.Equal(
				new string[] { "1" },
				selectionDSL
					.Commits().AreNotBugFixes()
					.Select(x => x.Revision));
		}
	}
}
