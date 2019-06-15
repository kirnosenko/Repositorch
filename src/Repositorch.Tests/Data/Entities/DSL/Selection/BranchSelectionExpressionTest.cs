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
		[Fact]
		public void Should_select_commits_relatively_specified()
		{
			mappingDSL
				.AddCommit("100").OnBranch(0b1000_0000)
			.Submit()
				.AddCommit("110").OnBranch(0b0000_0001, 7)
			.Submit()
				.AddCommit("200").OnBranch(0b0001_0001, 7)
			.Submit()
				.AddCommit("300").OnBranch(0b0100_0001, 7)
			.Submit()
				.AddCommit("210").OnBranch(0b0011_0001, 11)
			.Submit()
				.AddCommit("310").OnBranch(0b1100_0100, 11)
			.Submit()
				.AddCommit("999").OnBranch(0b1111_0101, 11)
			.Submit();
			
			Assert.Equal(new string[] { }, selectionDSL
				.Commits().BeforeRevision("100").Select(c => c.Revision));
			Assert.Equal(new string[] { "100" }, selectionDSL
				.Commits().BeforeRevision("110").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110" }, selectionDSL
				.Commits().BeforeRevision("200").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110" }, selectionDSL
				.Commits().BeforeRevision("300").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110", "200" }, selectionDSL
				.Commits().BeforeRevision("210").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110", "300" }, selectionDSL
				.Commits().BeforeRevision("310").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110", "200", "300", "210", "310" }, selectionDSL
				.Commits().BeforeRevision("999").Select(c => c.Revision));

			Assert.Equal(new string[] { "100" }, selectionDSL
				.Commits().TillRevision("100").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110" }, selectionDSL
				.Commits().TillRevision("110").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110", "200" }, selectionDSL
				.Commits().TillRevision("200").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110", "300" }, selectionDSL
				.Commits().TillRevision("300").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110", "200", "210" }, selectionDSL
				.Commits().TillRevision("210").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110", "300", "310" }, selectionDSL
				.Commits().TillRevision("310").Select(c => c.Revision));
			Assert.Equal(new string[] { "100", "110", "200", "300", "210", "310", "999" }, selectionDSL
				.Commits().TillRevision("999").Select(c => c.Revision));

			Assert.Equal(new string[] { "110", "200", "300", "210", "310", "999" }, selectionDSL
				.Commits().AfterRevision("100").Select(c => c.Revision));
			Assert.Equal(new string[] { "200", "300", "210", "310", "999" }, selectionDSL
				.Commits().AfterRevision("110").Select(c => c.Revision));
			Assert.Equal(new string[] { "210", "999" }, selectionDSL
				.Commits().AfterRevision("200").Select(c => c.Revision));
			Assert.Equal(new string[] { "310", "999" }, selectionDSL
				.Commits().AfterRevision("300").Select(c => c.Revision));
			Assert.Equal(new string[] { "999" }, selectionDSL
				.Commits().AfterRevision("210").Select(c => c.Revision));
			Assert.Equal(new string[] { "999" }, selectionDSL
				.Commits().AfterRevision("310").Select(c => c.Revision));
			Assert.Equal(new string[] { }, selectionDSL
				.Commits().AfterRevision("999").Select(c => c.Revision));

			Assert.Equal(new string[] { "100", "110", "200", "300", "210", "310", "999" }, selectionDSL
				.Commits().FromRevision("100").Select(c => c.Revision));
			Assert.Equal(new string[] { "110", "200", "300", "210", "310", "999" }, selectionDSL
				.Commits().FromRevision("110").Select(c => c.Revision));
			Assert.Equal(new string[] { "200", "210", "999" }, selectionDSL
				.Commits().FromRevision("200").Select(c => c.Revision));
			Assert.Equal(new string[] { "300", "310", "999" }, selectionDSL
				.Commits().FromRevision("300").Select(c => c.Revision));
			Assert.Equal(new string[] { "210", "999" }, selectionDSL
				.Commits().FromRevision("210").Select(c => c.Revision));
			Assert.Equal(new string[] { "310", "999" }, selectionDSL
				.Commits().FromRevision("310").Select(c => c.Revision));
			Assert.Equal(new string[] { "999" }, selectionDSL
				.Commits().FromRevision("999").Select(c => c.Revision));
		}
		[Fact]
		public void Should_combine_commits_relatively_specified()
		{
			mappingDSL
				.AddCommit("100").OnBranch(0b1000_0000)
			.Submit()
				.AddCommit("110").OnBranch(0b0000_0001, 7)
			.Submit()
				.AddCommit("200").OnBranch(0b0001_0001, 7)
			.Submit()
				.AddCommit("300").OnBranch(0b0100_0001, 7)
			.Submit()
				.AddCommit("210").OnBranch(0b0011_0001, 11)
			.Submit()
				.AddCommit("310").OnBranch(0b1100_0100, 11)
			.Submit()
				.AddCommit("999").OnBranch(0b1111_0101, 11)
			.Submit();

			Assert.Equal(new string[] { "200", "210", "999" }, selectionDSL
				.Commits().FromRevision("200").TillRevision("999")
				.Select(c => c.Revision));
			Assert.Equal(new string[] { }, selectionDSL
				.Commits().FromRevision("999").TillRevision("200")
				.Select(c => c.Revision));
		}
		[Fact]
		public void Should_ignore_null_values_for_commit_relative_selection()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit();

			Assert.Equal(3, selectionDSL.Commits()
				.AfterRevision(null)
				.FromRevision(null)
				.TillRevision(null)
				.BeforeRevision(null)
				.Count());
		}
	}
}
