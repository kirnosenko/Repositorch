using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class CodeBlockSelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_select_codeblocks_for_modifications()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
						.Code(+100)
					.AddFile("file2").Modified()
						.Code(+50)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(+10)
						.Code(-5)
			.Submit();

			Assert.Equal(new double[] { 100, 10, -5 }, selectionDSL
				.Files().PathIs("file1")
				.Modifications().InFiles()
				.CodeBlocks().InModifications()
				.Select(x => x.Size));
			Assert.Equal(new double[] { 50 }, selectionDSL
				.Files().PathIs("file2")
				.Modifications().InFiles()
				.CodeBlocks().InModifications()
				.Select(x => x.Size));
		}
		[Fact]
		public void Should_select_codeblocks_modified_by_specified_codeblocks()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
						.Code(+100)
					.AddFile("file2").Modified()
						.Code(+50)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(+10)
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
			.Submit()
				.AddCommit("3")
					.File("file1").Modified()
						.Code(+20)
						.Code(-2).ForCodeAddedInitiallyInRevision("1")
						.Code(-3).ForCodeAddedInitiallyInRevision("2")
					.File("file2").Modified()
						.Code(-4).ForCodeAddedInitiallyInRevision("1")
			.Submit();

			Assert.Equal(new double[] { 100, 10 }, selectionDSL
				.Commits().RevisionIs("3")
				.Files().PathIs("file1")
				.Modifications().InCommits().InFiles()
				.CodeBlocks().InModifications().Modify()
				.Select(x => x.Size));
			Assert.Equal(new double[] { -5, -2, -4 }, selectionDSL
				.Commits().RevisionIs("1")
				.Modifications().InCommits()
				.CodeBlocks().InModifications().ModifiedBy()
				.Select(x => x.Size));
		}
		[Fact]
		public void Should_select_code_in_bugfixes()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
						.Code(100)
			.Submit()
				.AddCommit("2").IsBugFix()
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
						.Code(5)
			.Submit()
				.AddCommit("3")
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(20)
			.Submit()
				.AddCommit("4").IsBugFix()
					.File("file1").Modified()
						.Code(-3)
						.Code(3)
			.Submit();

			Assert.Equal(new double[] { -5, 5, -3, 3 }, selectionDSL
				.CodeBlocks().InBugFixes()
				.Select(x => x.Size));
			Assert.Equal(new double[] { -5, 5 }, selectionDSL
				.Commits().BeforeNumber(4)
				.BugFixes().InCommits()
				.CodeBlocks().InBugFixes()
				.Select(x => x.Size));
		}
		[Fact]
		public void Should_select_code_added_initially_in_commit()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.AddFile("file2").CopiedFrom("file1", "1").Modified()
						.CopyCode()
			.Submit();

			Assert.Equal(new double[] { 100, 100 }, selectionDSL
				.Commits().RevisionIs("1")
				.CodeBlocks().AddedInitiallyInCommits()
				.Select(x => x.Size));
		}
		[Fact]
		public void Should_select_unique_modifications_that_contain_codeblocks()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(-10)
						.Code(15)
			.Submit();

			Assert.Equal(2, selectionDSL
				.CodeBlocks().Added()
				.Modifications().ContainCodeBlocks().Count());
			Assert.Equal(1, selectionDSL
				.CodeBlocks().Deleted()
				.Modifications().ContainCodeBlocks().Count());
			Assert.Equal(2, selectionDSL
				.Modifications().ContainCodeBlocks().Count());
		}
		[Fact]
		public void Should_select_refactoring_commits()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(10)
			.Submit()
				.AddCommit("3").IsBugFix()
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
						.Code(10)
			.Submit()
				.AddCommit("4")
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
						.Code(15)
			.Submit();

			Assert.Equal(new string[] { "2" }, selectionDSL
				.Commits().AreRefactorings()
				.Select(c => c.Revision));
		}
	}
}
