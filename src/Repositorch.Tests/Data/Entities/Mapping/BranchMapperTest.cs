using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class BranchMapperTest : BaseMapperTest
	{
		private BranchMapper mapper;
		
		public BranchMapperTest()
		{
			mapper = new BranchMapper(vcsData);
		}
		[Fact]
		public void Should_add_branch_for_commit()
		{
			vcsData.Log("1")
				.Returns(new TestLog("1"));

			mapper.Map(
				mappingDSL.AddCommit("1")
			);
			SubmitChanges();

			Assert.Equal(1, Get<Branch>().Count());
			var branch = Get<Branch>().Single();
			Assert.Equal("1", branch.Mask.Data);
			Assert.Equal(0, branch.Mask.Offset);
		}
		[Fact]
		public void Should_use_the_same_branch_for_sequential_commits()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit();

			vcsData.Log("1")
				.Returns(new TestLog("1").ChildRevisionsAre("2"));
			vcsData.Log("2")
				.Returns(new TestLog("2").ParentRevisionsAre("1"));

			mapper.Map(
				mappingDSL.AddCommit("2")
			);
			SubmitChanges();

			Assert.Equal(1, Get<Branch>().Count());
			int branchId = Get<Branch>().Single().Id;
			Assert.Equal(new int[] { branchId, branchId }, Get<Commit>()
				.Select(c => c.BranchId));
		}
		[Fact]
		public void Should_add_a_new_subbranch_with_new_mask_for_each_child_of_multichildren_commit()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit();

			vcsData.Log("2")
				.Returns(new TestLog("2").ParentRevisionsAre("1"));
			vcsData.Log("3")
				.Returns(new TestLog("3").ParentRevisionsAre("1"));
			vcsData.Log("1")
				.Returns(new TestLog("1").ChildRevisionsAre("2", "3"));

			mapper.Map(
				mappingDSL.AddCommit("2")
			);
			SubmitChanges();
			mapper.Map(
				mappingDSL.AddCommit("3")
			);
			SubmitChanges();

			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new string[] { "1", "1", "101" }, Get<Branch>()
				.Select(b => b.Mask.Data));
			Assert.Equal(new int[] { 0, 1, 0 }, Get<Branch>()
				.Select(b => b.Mask.Offset));
		}
		[Fact]
		public void Should_create_a_new_branch_for_nonfirst_commit_without_parents()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit();

			vcsData.Log("2")
				.Returns(new TestLog("2").ParentRevisionsAre(new string[] { }));
			vcsData.Log("1")
				.Returns(new TestLog("1").ChildRevisionsAre("3"));

			mapper.Map(
				mappingDSL.AddCommit("2")
			);
			SubmitChanges();
			
			Assert.Equal(2, Get<Branch>().Count());
			Assert.Equal(new string[] { "1", "01" }, Get<Branch>()
				.Select(b => b.Mask.Data));
			Assert.Equal(new int[] { 0, 0 }, Get<Branch>()
				.Select(b => b.Mask.Offset));
		}
		[Fact]
		public void Should_combine_branch_mask_from_parents_branch_masks()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit()
				.AddCommit("2").OnBranch("01")
			.Submit();

			vcsData.Log("3")
				.Returns(new TestLog("3").ParentRevisionsAre("1", "2"));
			vcsData.Log("1")
				.Returns(new TestLog("1").ChildRevisionsAre("3"));
			vcsData.Log("2")
				.Returns(new TestLog("2").ChildRevisionsAre("3"));

			mapper.Map(
				mappingDSL.AddCommit("3")
			);
			SubmitChanges();

			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new string[] { "1", "01", "1" }, Get<Branch>()
				.Select(b => b.Mask.Data));
			Assert.Equal(new int[] { 0, 0, 1 }, Get<Branch>()
				.Select(b => b.Mask.Offset));
		}
		[Fact]
		public void Should_change_combined_mask_when_parents_have_another_child()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
			.Submit()
				.AddCommit("2").OnBranch("01")
			.Submit();

			vcsData.Log("3")
				.Returns(new TestLog("3").ParentRevisionsAre("1", "2"));
			vcsData.Log("1")
				.Returns(new TestLog("1").ChildRevisionsAre("3"));
			vcsData.Log("2")
				.Returns(new TestLog("2").ChildRevisionsAre("3", "4"));

			mapper.Map(
				mappingDSL.AddCommit("3")
			);
			SubmitChanges();

			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new string[] { "1", "01", "1" }, Get<Branch>()
				.Select(b => b.Mask.Data));
			Assert.Equal(new int[] { 0, 0, 2 }, Get<Branch>()
				.Select(b => b.Mask.Offset));
		}
	}
}
