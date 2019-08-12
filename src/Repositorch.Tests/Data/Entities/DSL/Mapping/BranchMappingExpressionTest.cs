﻿using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class BranchMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_create_a_fresh_branch()
		{
			mappingDSL
				.AddCommit("1").OnFreshBranch()
			.Submit();

			Assert.Equal(1, Get<Branch>().Count());
			var branch = Get<Branch>().Single();
			Assert.Equal("1", branch.Mask.Data);
			Assert.Equal(0, branch.Mask.Offset);
		}
		[Fact]
		public void Should_create_a_fresh_branch_when_there_are_another_branches()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit()
				.AddCommit("2").OnBranch("11")
			.Submit()
				.AddCommit("3").OnBranch("101")
			.Submit()
				.AddCommit("4").OnFreshBranch()
			.Submit();

			Assert.Equal(4, Get<Branch>().Count());
			Assert.Equal(new string[] { "1", "11", "101", "0001" },
				Get<Branch>().Select(x => x.Mask.Data));
			Assert.Equal(new int[] { 0, 0, 0, 0 },
				Get<Branch>().Select(x => x.Mask.Offset));
		}
		[Fact]
		public void Should_add_a_branch_or_use_the_existent()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit()
				.AddCommit("2").OnBranch("1")
			.Submit()
				.AddCommit("3").OnBranch("11")
			.Submit()
				.AddCommit("4").OnBranch("101")
			.Submit();
			
			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new string[] { "1", "1", "11", "101" }, 
				from c in Get<Commit>()
				join b in Get<Branch>() on c.BranchId equals b.Id
				select b.Mask.Data);
		}
		[Fact]
		public void Should_add_a_new_subbranch_based_on_parent_mask()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit()
				.AddCommit("2").OnBranch("11")
			.Submit()
				.AddCommit("3").OnBranch("101")
			.Submit()
				.AddCommit("4").OnSubBranch("101")
			.Submit()
				.AddCommit("5").OnSubBranch("11")
			.Submit();

			Assert.Equal(5, Get<Branch>().Count());
			Assert.Equal(new string[] { "1", "11", "101", "1011", "1001" },
				Get<Branch>().Select(b => b.Mask.Data));
			Assert.Equal(new int[] { 0, 0, 0, 0, 1 },
				Get<Branch>().Select(b => b.Mask.Offset));
		}
		[Fact]
		public void Should_check_existing_branch_masks_while_creating_a_new_subbranch_mask()
		{
			mappingDSL
				.AddCommit("1").OnBranch("101")
			.Submit()
				.AddCommit("2").OnBranch("1001")
			.Submit()
				.AddCommit("3").OnSubBranch("101")
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new string[] { "101", "1001", "10101" },
				Get<Branch>().Select(b => b.Mask.Data));
			Assert.Equal(new int[] { 0, 0, 0 },
				Get<Branch>().Select(b => b.Mask.Offset));
		}
		[Fact]
		public void Should_create_subbranch_from_combined_mask()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit()
				.AddCommit("2").OnBranch("01")
			.Submit()
				.AddCommit("3").OnSubBranch("11")
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			var branch = Get<Branch>().Last();
			Assert.Equal("1", branch.Mask.Data);
			Assert.Equal(2, branch.Mask.Offset);
		}
		[Fact]
		public void Should_create_correct_combined_mask_for_branches_with_different_mask_offsets()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit()
				.AddCommit("2").OnBranch(("1", 1))
			.Submit()
				.AddCommit("3").OnSubBranch("1")
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			var branch = Get<Branch>().Last();
			Assert.Equal("101", branch.Mask.Data);
			Assert.Equal(0, branch.Mask.Offset);
		}
		[Fact]
		public void Should_create_correct_combined_mask_when_last_branch_mask_offset_is_not_the_max()
		{
			mappingDSL
				.AddCommit("1").OnBranch(("1", 18))
			.Submit()
				.AddCommit("2").OnBranch(("0101", 16))
			.Submit()
				.AddCommit("3").OnSubBranch(("11", 18))
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			var branch = Get<Branch>().Last();
			Assert.Equal("1", branch.Mask.Data);
			Assert.Equal(20, branch.Mask.Offset);
		}
	}
}
