using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class CommitAttributeSelectionExpressionTest : BaseRepositoryTest
	{
		public CommitAttributeSelectionExpressionTest()
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
		public void Should_select_commits_are_merges_or_not()
		{
			selectionDSL
				.Commits().AreMerges()
				.Select(x => x.Revision)
					.Should().BeEquivalentTo(new string[] { "4" });
			selectionDSL
				.Commits().AreNotMerges()
				.Select(x => x.Revision)
					.Should().BeEquivalentTo(new string[] { "1", "2", "3" });
		}
		[Fact]
		public void Should_select_commits_are_splits_or_not()
		{
			selectionDSL
				.Commits().AreSplits()
				.Select(x => x.Revision)
					.Should().BeEquivalentTo(new string[] { "1" });
			selectionDSL
				.Commits().AreNotSplits()
				.Select(x => x.Revision)
					.Should().BeEquivalentTo(new string[] { "2", "3", "4" });
		}
	}
}
