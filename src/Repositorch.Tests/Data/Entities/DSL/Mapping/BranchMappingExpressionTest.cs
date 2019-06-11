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
		public void Can_create_subbranch_from_combined_mask()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
			.Submit()
				.AddCommit("2").OnBranch(0b0010)
			.Submit()
				.AddCommit("3").OnSubBranch(0b0011)
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new uint[] { 0, 0, 0 },
				Get<Branch>().Select(b => b.MaskOffset));
			Assert.Equal(new uint[] { 0b0001, 0b0010, 0b0111 },
				Get<Branch>().Select(b => b.Mask));
		}
		[Fact]
		public void Can_not_create_subbranch_with_mask_overflow()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0xFFFFFFFF - 1)
			.Submit();

			Assert.Throws<InvalidOperationException>(() =>
				mappingDSL.AddCommit("2").OnSubBranch(0xFFFFFFFF - 1));
		}
	}
}
