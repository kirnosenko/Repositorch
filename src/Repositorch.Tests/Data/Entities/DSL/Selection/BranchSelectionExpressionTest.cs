using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class BranchSelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_select_commits_on_branch()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
				.AddCommit("2").OnBranch(0b0001)
				.AddCommit("3").OnBranch(0b0011)
				.AddCommit("4").OnBranch(0b0101)
				.AddCommit("5").OnBranch(0b0101)
				.AddCommit("6").OnBranch(0b0011)
			.Submit();
			
			Assert.Equal(new string[] { "1", "2" }, selectionDSL
				.Commits().OnBranch(0b0001).Select(x => x.Revision));
			Assert.Equal(new string[] { "1", "2", "3", "6" }, selectionDSL
				.Commits().OnBranch(0b0011).Select(x => x.Revision));
			Assert.Equal(new string[] { "1", "2", "4", "5" }, selectionDSL
				.Commits().OnBranch(0b0101).Select(x => x.Revision));
		}
		[Fact]
		public void Should_select_commits_on_branch_when_masks_are_too_far_from_each_other()
		{
			mappingDSL
				.AddCommit("100").OnBranch(0b0001)
				.AddCommit("200").OnBranch(0b0001, 40)
				.AddCommit("300").OnBranch(0b0001, 80)
			.Submit();

			Assert.Equal(new string[] { "100" }, selectionDSL
				.Commits().OnBranch(0b0001).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "200" }, selectionDSL
				.Commits().OnBranch(0b0001, 40).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "200", "300" }, selectionDSL
				.Commits().OnBranch(0b0001, 80).Select(x => x.Revision));
		}
		[Fact]
		public void Should_select_commits_on_branch_when_masks_are_partially_intersected()
		{
			mappingDSL
				.AddCommit("100").OnBranch(0b1000_0000)
				.AddCommit("110").OnBranch(0b0000_0001, 7)
				.AddCommit("200").OnBranch(0b0001_0001, 7)
				.AddCommit("300").OnBranch(0b0100_0001, 7)
				.AddCommit("210").OnBranch(0b0011_0001, 11)
				.AddCommit("310").OnBranch(0b1100_0100, 11)
			.Submit();

			Assert.Equal(new string[] { "100", "110" }, selectionDSL
				.Commits().OnBranch(0b1000_0000).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110" }, selectionDSL
				.Commits().OnBranch(0b0000_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "200" }, selectionDSL
				.Commits().OnBranch(0b0001_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "300" }, selectionDSL
				.Commits().OnBranch(0b0100_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "200", "210" }, selectionDSL
				.Commits().OnBranch(0b0011_0001, 11).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "300", "310" }, selectionDSL
				.Commits().OnBranch(0b1100_0100, 11).Select(x => x.Revision));
		}
	}
}
