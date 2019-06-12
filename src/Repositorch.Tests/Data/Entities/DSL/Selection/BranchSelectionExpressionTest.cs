using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class BranchSelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_select_commits_on_branch_back()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
				.AddCommit("2").OnBranch(0b0001)
				.AddCommit("3").OnBranch(0b0101)
				.AddCommit("4").OnBranch(0b1001)
				.AddCommit("5").OnBranch(0b1001)
				.AddCommit("6").OnBranch(0b0101)
			.Submit();
			
			Assert.Equal(new string[] { "1", "2" }, selectionDSL
				.Commits().OnBranchBack(0b0001).Select(x => x.Revision));
			Assert.Equal(new string[] { "1", "2", "3", "6" }, selectionDSL
				.Commits().OnBranchBack(0b0101).Select(x => x.Revision));
			Assert.Equal(new string[] { "1", "2", "4", "5" }, selectionDSL
				.Commits().OnBranchBack(0b1001).Select(x => x.Revision));
		}
		[Fact]
		public void Should_select_commits_on_branch_forward()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
				.AddCommit("2").OnBranch(0b0001)
				.AddCommit("3").OnBranch(0b0101)
				.AddCommit("4").OnBranch(0b1001)
				.AddCommit("5").OnBranch(0b1001)
				.AddCommit("6").OnBranch(0b0101)
			.Submit();

			Assert.Equal(new string[] { "1", "2", "3", "4", "5", "6" }, selectionDSL
				.Commits().OnBranchForward(0b0001).Select(x => x.Revision));
			Assert.Equal(new string[] { "3", "6" }, selectionDSL
				.Commits().OnBranchForward(0b0101).Select(x => x.Revision));
			Assert.Equal(new string[] { "4", "5" }, selectionDSL
				.Commits().OnBranchForward(0b1001).Select(x => x.Revision));
		}
		[Fact]
		public void Should_select_commits_on_branch_back_when_masks_are_too_far_from_each_other()
		{
			mappingDSL
				.AddCommit("100").OnBranch(0b0001)
				.AddCommit("200").OnBranch(0b0001, 40)
				.AddCommit("300").OnBranch(0b0001, 80)
			.Submit();

			Assert.Equal(new string[] { "100" }, selectionDSL
				.Commits().OnBranchBack(0b0001).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "200" }, selectionDSL
				.Commits().OnBranchBack(0b0001, 40).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "200", "300" }, selectionDSL
				.Commits().OnBranchBack(0b0001, 80).Select(x => x.Revision));
		}
		[Fact]
		public void Should_select_commits_on_branch_forward_when_masks_are_too_far_from_each_other()
		{
			mappingDSL
				.AddCommit("100").OnBranch(0b0001)
				.AddCommit("200").OnBranch(0b0001, 40)
				.AddCommit("300").OnBranch(0b0001, 80)
			.Submit();

			Assert.Equal(new string[] { "100", "200", "300" }, selectionDSL
				.Commits().OnBranchForward(0b0001).Select(x => x.Revision));
			Assert.Equal(new string[] { "200", "300" }, selectionDSL
				.Commits().OnBranchForward(0b0001, 40).Select(x => x.Revision));
			Assert.Equal(new string[] { "300" }, selectionDSL
				.Commits().OnBranchForward(0b0001, 80).Select(x => x.Revision));
		}
		[Fact]
		public void Should_select_commits_on_branch_back_when_masks_are_partially_intersected()
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
				.Commits().OnBranchBack(0b1000_0000).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110" }, selectionDSL
				.Commits().OnBranchBack(0b0000_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "200" }, selectionDSL
				.Commits().OnBranchBack(0b0001_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "300" }, selectionDSL
				.Commits().OnBranchBack(0b0100_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "200", "210" }, selectionDSL
				.Commits().OnBranchBack(0b0011_0001, 11).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "300", "310" }, selectionDSL
				.Commits().OnBranchBack(0b1100_0100, 11).Select(x => x.Revision));
		}
		[Fact]
		public void Should_select_commits_on_branch_forward_when_masks_are_partially_intersected()
		{
			mappingDSL
				.AddCommit("100").OnBranch(0b1000_0000)
				.AddCommit("110").OnBranch(0b0000_0001, 7)
				.AddCommit("200").OnBranch(0b0001_0001, 7)
				.AddCommit("300").OnBranch(0b0100_0001, 7)
				.AddCommit("210").OnBranch(0b0011_0001, 11)
				.AddCommit("310").OnBranch(0b1100_0100, 11)
			.Submit();

			Assert.Equal(new string[] { "100", "110", "200", "300", "210", "310" }, selectionDSL
				.Commits().OnBranchForward(0b1000_0000).Select(x => x.Revision));
			Assert.Equal(new string[] { "100", "110", "200", "300", "210", "310" }, selectionDSL
				.Commits().OnBranchForward(0b0000_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "200", "210" }, selectionDSL
				.Commits().OnBranchForward(0b0001_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "300", "310" }, selectionDSL
				.Commits().OnBranchForward(0b0100_0001, 7).Select(x => x.Revision));
			Assert.Equal(new string[] { "210" }, selectionDSL
				.Commits().OnBranchForward(0b0011_0001, 11).Select(x => x.Revision));
			Assert.Equal(new string[] { "310" }, selectionDSL
				.Commits().OnBranchForward(0b1100_0100, 11).Select(x => x.Revision));
		}
	}
}
