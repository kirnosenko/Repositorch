using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class CommitSelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_select_commits_by_date()
		{
			mappingDSL
				.AddCommit("1").At(DateTime.Today.AddDays(-9))
			.Submit()
				.AddCommit("2").At(DateTime.Today.AddDays(-8))
			.Submit()
				.AddCommit("5").At(DateTime.Today.AddDays(-5))
			.Submit();

			Assert.Equal(
				1,
				selectionDSL
					.Commits().AfterDate(DateTime.Today.AddDays(-8)).Count());
			Assert.Equal(
				2,
				selectionDSL
					.Commits().FromDate(DateTime.Today.AddDays(-8)).Count());
			Assert.Equal(
				1,
				selectionDSL
					.Commits().BeforeDate(DateTime.Today.AddDays(-8)).Count());
			Assert.Equal(
				2,
				selectionDSL
					.Commits().TillDate(DateTime.Today.AddDays(-8)).Count());
		}
		[Fact]
		public void Should_select_commits_relatively_specified()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit();

			Assert.Equal(0, selectionDSL
				.Commits().BeforeNumber(1).Count());
			Assert.Equal(1, selectionDSL
				.Commits().BeforeNumber(2).Count());
			Assert.Equal(1, selectionDSL
				.Commits().TillNumber(1).Count());
			Assert.Equal(3, selectionDSL
				.Commits().TillNumber(3).Count());
			Assert.Equal(2, selectionDSL
				.Commits().FromNumber(2).Count());
			Assert.Equal(1, selectionDSL
				.Commits().FromNumber(3).Count());
			Assert.Equal(2, selectionDSL
				.Commits().AfterNumber(1).Count());
			Assert.Equal(0, selectionDSL
				.Commits().AfterNumber(3).Count());
		}
	}
}
