using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class CodeBlockMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_add_code_block()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
						.Code(10)
			.Submit();

			Assert.Equal(1, Get<CodeBlock>().Count());
			var cb = Get<CodeBlock>().Single();
			Assert.Equal(10, cb.Size);
			Assert.NotNull(cb.Modification);
			Assert.Equal("1", cb.AddedInitiallyInCommit.Revision);
		}
		[Fact]
		public void Should_set_target_codeblock_for_new_one()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
					.File("file2").Added()
						.Code(40)
			.Submit()
				.AddCommit("3")
					.File("file1").Modified()
						.Code(50)
			.Submit()
				.AddCommit("4")
					.File("file1").Modified()
						.Code(10)
						.Code(-3).ForCodeAddedInitiallyInRevision("2")
						.Code(-2).ForCodeAddedInitiallyInRevision("3")
			.Submit();

			var targetModification =
				from cb in Get<CodeBlock>().Where(cb => cb.Size < 0)
				join tcb in Get<CodeBlock>() on cb.TargetCodeBlockId equals tcb.Id
				join m in Get<Modification>() on tcb.ModificationId equals m.Id
				select m;

			Assert.Equal(Enumerable.Repeat("file1", 3),
				from m in targetModification
				join f in Get<CodeFile>() on m.FileId equals f.Id
				select f.Path);
			(from m in targetModification
			join c in Get<Commit>() on m.CommitNumber equals c.Number
			select c.Revision)
				.Should().BeEquivalentTo(new string[] { "1", "2", "3" });
		}
		[Fact]
		public void Should_use_the_last_target_codeblock_when_there_are_several_of_them()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Removed()
						.RemoveCode()
					.File("file2").CopiedFrom("file1", "1")
						.Code(100).CopiedFrom("1")
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file1").CopiedFrom("file2", "2")
						.Code(100).CopiedFrom("1")
					.File("file2").Removed()
						.RemoveCode()
			.Submit()
				.AddCommit("4").OnBranch("1")
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
			.Submit();

			var codeBlock = Get<CodeBlock>().Last();
			Assert.Equal("1", codeBlock.TargetCodeBlock.AddedInitiallyInCommit.Revision);
			Assert.Equal("3", codeBlock.TargetCodeBlock.Modification.Commit.Revision);
		}
		[Fact]
		public void Should_clear_added_initially_in_commit_field_for_targeted_code_block()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(20)
						.Code(-20).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3")
					.File("file1").Modified()
						.Code(10).ForCodeAddedInitiallyInRevision("1")
			.Submit();

			var codeBlock = Get<CodeBlock>().Last();
			Assert.Null(codeBlock.AddedInitiallyInCommit);
		}
		[Fact]
		public void Should_set_correct_target_code_block_for_copied_files()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file2").CopiedFrom("file1", "1")
						.CopyCode()
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file2").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
			.Submit();

			Assert.Equal("2", Get<CodeBlock>()
				.Single(cb => cb.Size == -10)
				.TargetCodeBlock.Modification.Commit.Revision);
		}
		[Fact]
		public void Can_set_commit_code_was_added_in()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file2").CopiedFrom("file1", "2")
						.Code(95).CopiedFrom("1")
						.Code(20).CopiedFrom("2")
			.Submit()
				.AddCommit("4").OnBranch("1")
					.File("file3").CopiedFrom("file1", "2")
						.CopyCode()
			.Submit();

			Assert.Equal("1", Get<CodeBlock>()
				.Single(cb => cb.Modification.Commit.Revision == "1")
				.AddedInitiallyInCommit.Revision);
			Assert.Equal("2", Get<CodeBlock>()
				.Single(cb => cb.Modification.Commit.Revision == "2" && cb.Size > 0)
				.AddedInitiallyInCommit.Revision);
			Assert.Null(Get<CodeBlock>()
				.Single(cb => cb.Modification.Commit.Revision == "2" && cb.Size < 0)
				.AddedInitiallyInCommit);
			Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "3")
				.Select(cb => cb.AddedInitiallyInCommit.Revision)
					.Should().BeEquivalentTo(new string[] { "1", "2" });
			Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(cb => cb.AddedInitiallyInCommit.Revision)
					.Should().BeEquivalentTo(new string[] { "1", "2" });
		}
		[Fact]
		public void Should_copy_code_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file1").Modified()
						.Code(10)
						.Code(-2).ForCodeAddedInitiallyInRevision("2")
			.Submit()
				.AddCommit("4").OnBranch("1")
					.File("file2").CopiedFrom("file1", "2")
						.CopyCode()
			.Submit();

			Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(cb => cb.Size)
					.Should().BeEquivalentTo(new double[] { 95, 20 });
		}
		[Fact]
		public void Should_not_add_empty_code_blocks_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(10)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file1").Modified()
						.Code(5)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("4").OnBranch("1")
					.File("file2").CopiedFrom("file1", "3")
						.CopyCode()
			.Submit();

			Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(x => x.Size)
					.Should().BeEquivalentTo(new double[] { 20, 5 });
		}
		[Fact]
		public void Should_find_target_codeblock_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file2").CopiedFrom("file1", "1")
						.CopyCode()
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file2").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit();

			Assert.Equal("1", Get<CodeBlock>().Single(cb => cb.Size == -5)
				.TargetCodeBlock.AddedInitiallyInCommit.Revision);
		}
		[Fact]
		public void Should_copy_code_from_specified_file_only()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
					.File("file2").Added()
						.Code(50)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file3").CopiedFrom("file2", "1")
						.CopyCode()
			.Submit();

			Assert.Equal(new double[] { 50 }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "2")
				.Select(cb => cb.Size));
		}
		[Fact]
		public void Should_copy_code_with_correct_initial_commit()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(20)
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file2").CopiedFrom("file1", "2")
						.CopyCode()
			.Submit()
				.AddCommit("4").OnBranch("1")
					.File("file3").CopiedFrom("file2", "3")
						.CopyCode()
			.Submit();

			Assert.Equal(new string[] { "1", "2" }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(cb => cb.AddedInitiallyInCommit.Revision));
		}
		[Fact]
		public void Should_copy_code_from_the_same_branch_only()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
						.Code(20)
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Modified()
						.Code(30)
			.Submit()
				.AddCommit("4").OnBranch("11")
					.File("file1").Modified()
						.Code(40)
			.Submit()
				.AddCommit("5").OnBranch("11")
					.File("file2").CopiedFrom("file1", "4")
						.CopyCode()
			.Submit();

			Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "5")
				.Select(cb => cb.Size)
					.Should().BeEquivalentTo(new double[] { 100, 20, 40 });
		}
		[Fact]
		public void Should_remove_code_for_removed_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
						.Code(10)
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file1").Removed()
						.RemoveCode()
			.Submit();

			var codeBlocks = Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "3");

			codeBlocks.Count()
				.Should().Be(2);
			codeBlocks.Select(cb => cb.Size)
				.Should().BeEquivalentTo(new double[] { -95, -10 });
			codeBlocks.Select(cb => cb.TargetCodeBlock)
				.Should().NotContainNulls();
			codeBlocks.Select(cb => cb.TargetCodeBlock.Size)
				.Should().BeEquivalentTo(new double[] { 100, 10 });
		}
		[Fact]
		public void Should_not_add_empty_code_blocks_for_removed_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(10)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file1").Modified()
						.Code(5)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("4").OnBranch("1")
					.File("file1").Removed()
						.RemoveCode()
			.Submit();

			Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(x => x.Size)
					.Should().BeEquivalentTo(new double[] { -20, -5 });
		}
		[Fact]
		public void Can_remove_code_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file2").CopiedFrom("file1", "1")
						.CopyCode()
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file2").Removed()
						.RemoveCode()
			.Submit();

			Assert.Equal(0, Get<CodeBlock>()
				.Where(cb => cb.Modification.File.Path == "file2")
				.Sum(x => x.Size));
		}
		[Fact]
		public void Should_remove_code_from_the_same_branch_only()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
						.Code(20)
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Modified()
						.Code(30)
			.Submit()
				.AddCommit("4").OnBranch("101")
					.File("file1").Removed()
						.RemoveCode()
			.Submit();

			Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(cb => cb.Size)
					.Should().BeEquivalentTo(new double[] { -100, -30 });
		}
	}
}
