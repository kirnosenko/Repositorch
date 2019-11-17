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
				.AddCommit("1").OnBranch("1")
			.Submit()
				.AddCommit("2").OnBranch("1").HasTag("1.0")
			.Submit()
				.AddCommit("3").OnBranch("1").HasTags("1.1", "fix")
			.Submit();
		}
		[Fact]
		public void Should_select_tags_of_commits()
		{
			selectionDSL
				.Commits().RevisionIs("1")
				.Tags().OfCommits().Count()
					.Should().Be(0);
			selectionDSL
				.Commits().RevisionIs("2")
				.Tags().OfCommits().Count()
					.Should().Be(1);
			selectionDSL
				.Commits().RevisionIs("3")
				.Tags().OfCommits().Count()
					.Should().Be(2);
		}
		[Fact]
		public void Should_select_tags_by_title()
		{
			selectionDSL
				.Tags().WithTitles("1.0").Count()
					.Should().Be(1);
			selectionDSL
				.Tags().WithTitles("2.0").Count()
					.Should().Be(0);
			selectionDSL
				.Tags().WithTitles("1.1", "fix").Count()
					.Should().Be(2);
		}
		[Fact]
		public void Should_select_commits_with_tags()
		{
			selectionDSL
				.Commits().WithTag("2.0").Count()
					.Should().Be(0);
			selectionDSL
				.Tags().WithTitles("1.0", "2.0")
				.Commits().WithTags().Count()
					.Should().Be(1);
			selectionDSL
				.Tags().WithTitles("1.1", "fix")
				.Commits().WithTags().Count()
					.Should().Be(1);
		}
		[Fact]
		public void Should_select_commits_relatively_specified()
		{
			Assert.Equal(new string[] { "1" }, selectionDSL
				.Commits().BeforeTag("1.0").Select(c => c.Revision));	
			Assert.Equal(new string[] { "1", "2" }, selectionDSL
				.Commits().BeforeTag("fix").Select(c => c.Revision));
			Assert.Equal(new string[] { "1", "2", "3" }, selectionDSL
				.Commits().BeforeTag("2.0").Select(c => c.Revision));

			Assert.Equal(new string[] { "1", "2" }, selectionDSL
				.Commits().TillTag("1.0").Select(c => c.Revision));
			Assert.Equal(new string[] { "1", "2", "3" }, selectionDSL
				.Commits().TillTag("1.1").Select(c => c.Revision));
			Assert.Equal(new string[] { "1", "2", "3" }, selectionDSL
				.Commits().TillTag("2.0").Select(c => c.Revision));
			
			Assert.Equal(new string[] { "3" }, selectionDSL
				.Commits().AfterTag("1.0").Select(c => c.Revision));
			Assert.Equal(new string[] { }, selectionDSL
				.Commits().AfterTag("1.1").Select(c => c.Revision));
			Assert.Equal(new string[] { "1", "2", "3" }, selectionDSL
				.Commits().AfterTag("2.0").Select(c => c.Revision));
			
			Assert.Equal(new string[] { "2", "3" }, selectionDSL
				.Commits().FromTag("1.0").Select(c => c.Revision));
			Assert.Equal(new string[] { "3" }, selectionDSL
				.Commits().FromTag("1.1").Select(c => c.Revision));
			Assert.Equal(new string[] { "1", "2", "3" }, selectionDSL
				.Commits().FromTag("2.0").Select(c => c.Revision));
		}
	}
}
