using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class BugFixSelectionExpressionTest : BaseRepositoryTest
	{
		public BugFixSelectionExpressionTest()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1").IsSplit()
			.Submit()
				.AddCommit("2").OnBranch("11").IsBugFix().HasTag("1.0")
			.Submit()
				.AddCommit("3").OnBranch("101").IsBugFix()
			.Submit()
				.AddCommit("4").OnBranch("111").IsMerge().HasTags("1.1", "final")
			.Submit();
		}

		[Fact]
		public void Should_select_commits_are_bugfixes_or_not()
		{
			selectionDSL
				.Commits().AreBugFixes()
				.Select(x => x.Revision)
					.Should().BeEquivalentTo(new string[] { "2", "3" });
			selectionDSL
				.Commits().AreNotBugFixes()
				.Select(x => x.Revision)
					.Should().BeEquivalentTo(new string[] { "1", "4" });
		}

	}
}
