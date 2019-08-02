using System;
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
			Assert.Equal(1u, branch.Mask);
			Assert.Equal(0u, branch.MaskOffset);
		}
		[Fact]
		public void Should_create_a_fresh_branch_when_there_are_another_branches()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
			.Submit()
				.AddCommit("2").OnBranch(0b0101)
			.Submit()
				.AddCommit("3").OnBranch(0b1101)
			.Submit()
				.AddCommit("4").OnFreshBranch()
			.Submit();

			Assert.Equal(4, Get<Branch>().Count());
			Assert.Equal(new uint[] { 0b0001, 0b0101, 0b1101, 0b10000 },
				Get<Branch>().Select(x => x.Mask));
		}
		[Fact]
		public void Should_add_a_branch_or_use_the_existent()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
			.Submit()
				.AddCommit("2").OnBranch(0b0001)
			.Submit()
				.AddCommit("3").OnBranch(0b0101)
			.Submit()
				.AddCommit("4").OnBranch(0b1101)
			.Submit();
			
			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new uint[] { 0b0001, 0b0001, 0b0101, 0b1101 }, 
				from c in Get<Commit>()
				join b in Get<Branch>() on c.BranchId equals b.Id
				select b.Mask);
		}
		[Fact]
		public void Should_shift_mask_when_it_is_possible()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
			.Submit()
				.AddCommit("2").OnBranch(0b0011)
			.Submit()
				.AddCommit("3").OnBranch(0b0111)
			.Submit()
				.AddCommit("4").OnBranch(0b1011)
			.Submit();

			Assert.Equal(4, Get<Branch>().Count());
			Assert.Equal(new uint[] { 0b0001, 0b0001, 0b0001, 0b0101 },
				Get<Branch>().Select(b => b.Mask));
			Assert.Equal(new uint[] { 0, 1, 2, 1 },
				Get<Branch>().Select(b => b.MaskOffset));
		}
		[Fact]
		public void Should_add_a_new_subbranch_based_on_parent_mask()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0101)
			.Submit()
				.AddCommit("2").OnSubBranch(0b0101)
			.Submit();

			Assert.Equal(2, Get<Branch>().Count());
			Assert.Equal(new uint[] { 0b0101, 0b1101 },
				Get<Branch>().Select(b => b.Mask));
			Assert.Equal(new uint[] { 0, 0 },
				Get<Branch>().Select(b => b.MaskOffset));
		}
		[Fact]
		public void Should_check_existing_branch_masks_while_creating_a_new_subbranch_mask()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0101)
			.Submit()
				.AddCommit("2").OnBranch(0b1001)
			.Submit()
				.AddCommit("3").OnSubBranch(0b0101)
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new uint[] { 0, 0, 0 },
				Get<Branch>().Select(b => b.MaskOffset));
			Assert.Equal(new uint[] { 0b0101, 0b1001, 0b10101 },
				Get<Branch>().Select(b => b.Mask));
		}
		[Fact]
		public void Should_get_correct_max_branch_mask_in_case_of_shifted_mask()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0101)
			.Submit()
				.AddCommit("2").OnBranch(0b0001, 2)
			.Submit()
				.AddCommit("3").OnSubBranch(0b0001, 2)
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			var branch = Get<Branch>().Last();
			Assert.Equal(0b0001u, branch.Mask);
			Assert.Equal(3u, branch.MaskOffset);
		}
		[Fact]
		public void Should_create_subbranch_from_combined_mask()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
			.Submit()
				.AddCommit("2").OnBranch(0b0010)
			.Submit()
				.AddCommit("3").OnSubBranch(0b0011)
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			var branch = Get<Branch>().Last();
			Assert.Equal(0b0001u, branch.Mask);
			Assert.Equal(2u, branch.MaskOffset);
		}
		[Fact]
		public void Should_create_correct_combined_mask_for_branches_with_different_mask_offsets()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
			.Submit()
				.AddCommit("2").OnBranch(0b0011)
			.Submit()
				.AddCommit("3").OnSubBranch(0b0001)
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			var branch = Get<Branch>().Last();
			Assert.Equal(0b0101u, branch.Mask);
			Assert.Equal(0u, branch.MaskOffset);
		}
		[Fact]
		public void Should_create_correct_combined_mask_when_last_branch_mask_offset_is_not_the_max()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001, 18)
			.Submit()
				.AddCommit("2").OnBranch(0b1010, 16)
			.Submit()
				.AddCommit("3").OnSubBranch(0b0011, 18)
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			var branch = Get<Branch>().Last();
			Assert.Equal(1u, branch.Mask);
			Assert.Equal(20u, branch.MaskOffset);
		}
		[Fact]
		public void Should_check_mask_overflow_while_creating_subbranch()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0xFFFFFFFF - 1)
			.Submit();

			Assert.Throws<InvalidOperationException>(() =>
				mappingDSL.AddCommit("2").OnSubBranch(0xFFFFFFFF - 1));
		}
		[Fact]
		public void Should_check_mask_overflow_in_case_of_different_mask_offsets()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0x0FFFFFFE, 0)
			.Submit()
				.AddCommit("2").OnBranch(0x0FFFFFFE, 10)
			.Submit();
			
			Assert.Throws<InvalidOperationException>(() =>
				mappingDSL.AddCommit("2").OnSubBranch(0x0FFFFFFE));
		}
	}
}
