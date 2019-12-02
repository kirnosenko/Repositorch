using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeBlockMapperTest : BaseMapperTest
	{
		private CodeBlockMapper mapper;
		
		public CodeBlockMapperTest()
		{
			mapper = new CodeBlockMapper(vcsData);
		}
		[Fact]
		public void Should_map_added_lines()
		{
			vcsData.Log("abc")
				.Returns(new TestLog("abc"));
			vcsData.Blame("abc", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("abc", 3));

			mapper.Map(
				mappingDSL.AddCommit("abc").OnBranch("1")
					.File("file1").Added().Modified()
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeBlock>().Count());
			var cb = Get<CodeBlock>().Single();
			Assert.Equal(3, cb.Size);
			Assert.Equal("abc", cb.AddedInitiallyInCommit.Revision);
		}
		[Fact]
		public void Should_not_take_blame_for_deleted_file()
		{
			mappingDSL
				.AddCommit("ab").OnBranch("1")
					.File("file1").Added()
						.Code(100)
				.Submit();
			mapper.Map(
				mappingDSL.AddCommit("abc").OnBranch("1")
					.File("file1").Removed()
			);
			SubmitChanges();

			Get<CodeBlock>().Select(cb => cb.Size)
				.Should().BeEquivalentTo(new double[] { 100, -100 });
			vcsData.DidNotReceive().Blame(Arg.Any<string>(), Arg.Any<string>());
		}
		[Fact]
		public void Should_remove_code_that_no_more_exists()
		{
			mappingDSL
				.AddCommit("a").OnBranch("1")
					.File("file1").Added()
						.Code(10)
			.Submit()
				.AddCommit("ab").OnBranch("1")
					.File("file1").Modified()
						.Code(20)
			.Submit();

			vcsData.Log("abc")
				.Returns(new TestLog("abc"));
			vcsData.Blame("abc", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("abc", 10)
					.AddLinesFromRevision("ab", 15));

			mapper.Map(
				mappingDSL.AddCommit("abc").OnBranch("1")
					.File("file1").Modified()
			);
			SubmitChanges();

			var code = Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "abc");

			code.Select(cb => cb.Size)
				.Should().BeEquivalentTo(new double[] { 10, -10, -5 });
			code.Where(cb => cb.Size < 0).Select(cb => cb.TargetCodeBlock.Size)
				.Should().BeEquivalentTo(new double[] { 10, 20 });
		}
		[Fact]
		public void Should_not_take_into_account_code_on_another_branch()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
						.Code(10)
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Modified()
						.Code(20)
						.Code(-20).ForCodeAddedInitiallyInRevision("1")
			.Submit();

			vcsData.Log("4")
				.Returns(new TestLog("4"));
			vcsData.Blame("4", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 90)
					.AddLinesFromRevision("2", 5)
					.AddLinesFromRevision("4", 5));

			mapper.Map(
				mappingDSL.AddCommit("4").OnBranch("11")
					.File("file1").Modified()
			);
			SubmitChanges();

			var code = Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4");

			code.Select(cb => cb.Size)
				.Should().BeEquivalentTo(new double[] { 5, -5 });
			code.Where(cb => cb.Size < 0).Select(cb => cb.TargetCodeBlock.Size)
				.Should().BeEquivalentTo(new double[] { 10 });
		}
		[Fact]
		public void Should_map_all_code_as_is_for_copied_file()
		{
			mappingDSL
				.AddCommit("a").OnBranch("1")
					.File("file1").Added()
						.Code(10)
			.Submit()
				.AddCommit("ab").OnBranch("1")
					.File("file1").Modified()
						.Code(5)
			.Submit();

			vcsData.Blame("abc", "file2")
				.Returns(new TestBlame()
					.AddLinesFromRevision("a", 10)
					.AddLinesFromRevision("ab", 5));

			mapper.Map(
				mappingDSL.AddCommit("abc").OnBranch("1")
					.File("file2").CopiedFrom("file1", "ab")
			);
			SubmitChanges();

			var code = Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "abc");

			code.Select(cb => cb.Size)
				.Should().BeEquivalentTo(new double[] { 10, 5 });
			code.Select(cb => cb.AddedInitiallyInCommit.Revision)
				.Should().BeEquivalentTo(new string[] { "a", "ab" });
		}
		[Fact]
		public void Should_map_new_code_in_copied_file_as_new()
		{
			mappingDSL
				.AddCommit("a").OnBranch("1")
					.File("file1").Added()
						.Code(10)
			.Submit();

			vcsData.Blame("abc", "file2")
				.Returns(new TestBlame()
					.AddLinesFromRevision("a", 10)
					.AddLinesFromRevision("abc", 5));

			mapper.Map(
				mappingDSL.AddCommit("abc").OnBranch("1")
					.File("file2").CopiedFrom("file1", "a")
			);
			SubmitChanges();

			Assert.Equal("abc", Get<CodeBlock>()
				.Single(cb => cb.Size == 5)
				.AddedInitiallyInCommit.Revision);
		}
		[Fact]
		public void Should_compensate_code_changes_that_were_dropped_in_merge()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
						.Code(10)
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Modified()
						.Code(20)
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
			.Submit();

			vcsData.Log("4")
				.Returns(new TestLog("4").ParentRevisionsAre("2", "3"));
			vcsData.Blame("4", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 90)
					.AddLinesFromRevision("2", 5)
					.AddLinesFromRevision("3", 10));

			mapper.Map(
				mappingDSL.AddCommit("4").OnBranch("111")
					.File("file1").Modified()
			);
			SubmitChanges();

			var mergeCodeBlocks =
				(from cb in Get<CodeBlock>()
				join m in Get<Modification>() on cb.ModificationId equals m.Id
				join c in Get<Commit>() on m.CommitId equals c.Id
				where c.Revision == "4"
				select cb).ToArray();

			mergeCodeBlocks.Select(x => x.Size)
				.Should().BeEquivalentTo(new double[] { 10, -5, -10 });
			mergeCodeBlocks.Select(x => x.TargetCodeBlock.Size)
				.Should().BeEquivalentTo(new double[] { 100, 10, 20 });
			Assert.True(mergeCodeBlocks
				.Select(x => x.AddedInitiallyInCommit)
				.All(x => x == null));
		}
		[Fact]
		public void Should_not_map_removed_file_when_remove_modification_is_not_on_merged_branches()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
						.Code(10)
			.Submit()
				.AddCommit("3").OnBranch("11")
					.File("file1").Modified()
						.Code(10)
			.Submit()
				.AddCommit("4").OnBranch("101")
					.File("file1").Modified()
						.Code(10)
			.Submit()
				.AddCommit("5").OnBranch("101")
					.File("file1").Removed()
			.Submit()
				.AddCommit("6").OnBranch("111")
			.Submit()
				.AddCommit("7").OnBranch("1011")
			.Submit();

			vcsData.Log("10")
				.Returns(new TestLog("10").ParentRevisionsAre("6", "7"));
			vcsData.Blame("10", "file1")
				.Returns((TestBlame)null);

			mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("1111")
					.File("file1").Modified()
			);
			SubmitChanges();

			var modifications =
				(from m in Get<Modification>()
				 join c in Get<Commit>() on m.CommitId equals c.Id
				 where c.Revision == "10"
				 select m).ToArray();

			modifications.Count()
				.Should().Be(0);
		}
		[Fact]
		public void Should_take_into_account_possible_changes_in_renamed_file_from_original_file_in_merge()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file2").CopiedFrom("file1", "1")
						.CopyCode()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Modified()
						.Code(20)
						.Code(-20).ForCodeAddedInitiallyInRevision("1")
			.Submit();

			vcsData.Log("10")
				.Returns(new TestLog("10").ParentRevisionsAre("2", "3"));
			vcsData.Blame("10", "file2")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 80)
					.AddLinesFromRevision("3", 20));

			mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("111")
					.File("file2").Modified()
			);
			SubmitChanges();

			var blocks = Get<CodeBlock>()
				.Where(x => x.Modification.File.Path == "file2");
			blocks.Select(x => x.Size)
				.Should().BeEquivalentTo(new double[] { 100, -20, 20 });
			blocks.Select(x => x.AddedInitiallyInCommit != null ?
				x.AddedInitiallyInCommit.Revision : null)
				.Should().BeEquivalentTo(new string[] { "1", null, "3" });
		}
		[Fact]
		public void Should_revert_parent_modification_if_no_blocks_were_added()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
						.Code(-20).ForCodeAddedInitiallyInRevision("1")
						.Code(30)
			.Submit();

			vcsData.Log("3")
				.Returns(new TestLog("3"));
			vcsData.Blame("3", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 80)
					.AddLinesFromRevision("2", 30));

			mapper.Map(
				mappingDSL.AddCommit("3").OnBranch("1")
					.File("file1").Modified()
			);
			SubmitChanges();

			Get<CodeBlock>().Select(cb => cb.Size)
				.Should().BeEquivalentTo(new double[] { 100, -20, 30 });
			Get<Modification>().Count()
				.Should().Be(2);
		}
		[Fact]
		public void Should_not_revert_addition_or_removing_of_empty_file()
		{
			vcsData.Log("1")
				.Returns(new TestLog("1"));
			vcsData.Blame("1", "file1")
				.Returns(new TestBlame());

			mapper.Map(
				mappingDSL.AddCommit("1").OnBranch("1")
					.File("file1").Added()
			);
			SubmitChanges();
			mapper.Map(
				mappingDSL.AddCommit("2").OnBranch("1")
					.File("file1").Removed()
			);
			SubmitChanges();

			Get<CodeBlock>().Count()
				.Should().Be(0);
			Get<Modification>().Count()
				.Should().Be(2);
		}
	}
}
