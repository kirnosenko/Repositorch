using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class TagSelectionExpressionTest : BaseRepositoryTest
	{
		public TagSelectionExpressionTest()
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
		public void Should_select_commits_with_tags()
		{
			selectionDSL
				.Commits().WithTags("2.0").Select(c => c.Revision)
					.Should().BeEmpty();
			selectionDSL
				.Commits().WithTags("1.0", "2.0").Select(c => c.Revision)
					.Should().BeEquivalentTo(new string[] { "2" });
		}
		[Fact]
		public void Should_select_commits_relatively_specified()
		{
			selectionDSL
				.Commits().BeforeTag("1.0")
				.Select(c => c.Revision)
					.Should().BeEquivalentTo(new string[] { "1" });
			selectionDSL
				.Commits().BeforeTag("1.1")
				.Select(c => c.Revision)
					.Should().BeEquivalentTo(new string[] { "1", "2", "3" });
			selectionDSL
				.Commits().BeforeTag("2.0")
				.Select(c => c.Revision)
					.Should().BeEmpty();

			selectionDSL
				.Commits().TillTag("1.0")
				.Select(c => c.Revision)
					.Should().BeEquivalentTo(new string[] { "1", "2" });
			selectionDSL
				.Commits().TillTag("1.1")
				.Select(c => c.Revision)
					.Should().BeEquivalentTo(new string[] { "1", "2", "3", "4" });
			selectionDSL
				.Commits().TillTag("2.0")
				.Select(c => c.Revision)
					.Should().BeEmpty();

			selectionDSL
				.Commits().AfterTag("1.0")
				.Select(c => c.Revision)
					.Should().BeEquivalentTo(new string[] { "4" });
			selectionDSL
				.Commits().AfterTag("1.1")
				.Select(c => c.Revision)
					.Should().BeEmpty();
			selectionDSL
				.Commits().AfterTag("2.0")
				.Select(c => c.Revision)
					.Should().BeEmpty();

			selectionDSL
				.Commits().FromTag("1.0")
				.Select(c => c.Revision)
					.Should().BeEquivalentTo(new string[] { "2", "4" });
			selectionDSL
				.Commits().FromTag("1.1")
				.Select(c => c.Revision)
					.Should().BeEquivalentTo(new string[] { "4" });
			selectionDSL
				.Commits().FromTag("2.0")
				.Select(c => c.Revision)
					.Should().BeEmpty();
		}
	}
}
