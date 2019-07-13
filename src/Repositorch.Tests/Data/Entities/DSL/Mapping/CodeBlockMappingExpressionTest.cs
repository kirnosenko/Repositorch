using System;
using System.Linq;
using Xunit;

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
		public void Can_set_target_codeblock_for_new_one()
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
			Assert.Equal(new string[] { "1", "2", "3" },
				from m in targetModification
				join c in Get<Commit>() on m.CommitId equals c.Id
				select c.Revision);
		}
		[Fact]
		public void Should_set_correct_target_code_block_for_copied_files()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file2").CopiedFrom("file1", "1")
						.CopyCode()
			.Submit()
				.AddCommit("3").OnBranch(1)
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
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file2").CopiedFrom("file1", "2")
						.Code(95).CopiedFrom("1")
						.Code(20).CopiedFrom("2")
			.Submit()
				.AddCommit("4").OnBranch(1)
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
			Assert.Equal(new string[] { "1", "2" }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "3")
				.Select(cb => cb.AddedInitiallyInCommit.Revision));
			Assert.Equal(new string[] { "1", "2" }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(cb => cb.AddedInitiallyInCommit.Revision));
		}
		[Fact]
		public void Should_copy_code_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file1").Modified()
						.Code(10)
						.Code(-2).ForCodeAddedInitiallyInRevision("2")
			.Submit()
				.AddCommit("4").OnBranch(1)
					.File("file2").CopiedFrom("file1", "2")
						.CopyCode()
			.Submit();

			Assert.Equal(new double[] { 95, 20 }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(cb => cb.Size));
		}
		[Fact]
		public void Should_not_add_empty_code_blocks_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(10)
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file1").Modified()
						.Code(5)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("4").OnBranch(1)
					.File("file2").CopiedFrom("file1", "3")
						.CopyCode()
			.Submit();

			Assert.Equal(new double[] { 20, 5 }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(x => x.Size));
		}
		[Fact]
		public void Should_find_target_codeblock_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file2").CopiedFrom("file1", "1")
						.CopyCode()
			.Submit()
				.AddCommit("3").OnBranch(1)
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
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(100)
					.File("file2").Added()
						.Code(50)
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file3").CopiedFrom("file2", "1")
						.CopyCode()
			.Submit();

			Assert.Equal(new double[] { 50 }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "2")
				.Select(cb => cb.Size));
		}
		[Fact]
		public void Should_copy_copied_code_with_correct_initial_commit()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(20)
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file2").CopiedFrom("file1", "2")
						.CopyCode()
			.Submit()
				.AddCommit("4").OnBranch(1)
					.File("file3").CopiedFrom("file2", "3")
						.CopyCode()
			.Submit();

			Assert.Equal(new string[] { "1", "2" }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(cb => cb.AddedInitiallyInCommit.Revision));
		}
		[Fact]
		public void Should_delete_code_for_deleted_file()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
						.Code(10)
			.Submit()
				.AddCommit("3")
					.File("file1").Removed()
						.DeleteCode()
			.Submit();

			var codeBlocks = Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "3");

			Assert.Equal(2, codeBlocks.Count());
			Assert.Equal(new double[] { -95, -10 }, codeBlocks
				.Select(cb => cb.Size));
			Assert.False(codeBlocks
				.Select(cb => cb.TargetCodeBlock)
				.Contains(null));
			Assert.Equal(new double[] { 100, 10 }, codeBlocks
				.Select(cb => cb.TargetCodeBlock.Size));
		}
		[Fact]
		public void Should_not_add_empty_code_blocks_for_deleted_file()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
						.Code(10)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(20)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3")
					.File("file1").Modified()
						.Code(5)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("4")
					.File("file1").Removed()
						.DeleteCode()
			.Submit();

			Assert.Equal(new double[] { -20, -5 }, Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "4")
				.Select(x => x.Size));
		}
		[Fact]
		public void Can_delete_code_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file2").CopiedFrom("file1", "1")
						.CopyCode()
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file2").Removed()
						.DeleteCode()
			.Submit();

			Assert.Equal(0, Get<CodeBlock>()
				.Where(cb => cb.Modification.File.Path == "file2")
				.Sum(x => x.Size));
		}
	}
}
