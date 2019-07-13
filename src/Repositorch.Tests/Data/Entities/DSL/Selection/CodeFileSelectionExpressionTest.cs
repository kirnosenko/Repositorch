using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class CodeFileSelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_get_files_added_in_specified_commits()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file1").Modified()
					.File("file2").Added()
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file3").CopiedFrom("file1", "1")
			.Submit();

			Assert.Equal(new string[] { "file1", "file2" }, selectionDSL
				.Commits().TillNumber(2)
				.Files().AddedInCommits()
				.Select(x => x.Path));
			Assert.Equal(new string[] { "file3" }, selectionDSL
				.Commits().FromNumber(3)
				.Files().AddedInCommits()
				.Select(x => x.Path));
		}
		[Fact]
		public void Should_get_files_removed_in_specified_commits()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file1").Removed()
					.File("file2").CopiedFrom("file1", "1")
					.File("file3").Added()
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file3").Removed()
			.Submit();

			Assert.Equal(new string[] { "file1" }, selectionDSL
				.Commits().TillNumber(2)
				.Files().RemovedInCommits()
				.Select(x => x.Path));
			Assert.Equal(new string[] { "file3" }, selectionDSL
				.Commits().FromNumber(3)
				.Files().RemovedInCommits()
				.Select(x => x.Path));
		}
		[Fact]
		public void Should_get_files_by_id()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
					.File("file2").Added()
			.Submit()
				.AddCommit("2")
					.File("file2").Modified()
					.File("file3").Added()
			.Submit();
			
			foreach (var file in selectionDSL.Files())
			{
				Assert.Equal(new string[] { file.Path }, selectionDSL
					.Files().IdIs(file.Id)
					.Select(f => f.Path));
			}
		}
		[Fact]
		public void Should_get_files_by_name()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2")
					.File("file2").Added()
					.File("file1").Removed()
			.Submit()
				.AddCommit("3")
					.File("file1").Added()
			.Submit();

			Assert.Equal(1, selectionDSL
				.Files().PathIs("file1").Count());
			Assert.Equal(1, selectionDSL
				.Files().PathIs("file2").Count());
			Assert.Equal(0, selectionDSL
				.Files().PathIs("file3").Count());
		}
		[Fact]
		public void Should_get_existent_files()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2")
					.File("file1").Removed()
					.File("file2").Added()
			.Submit();

			Assert.Equal(new string[] { "file2" }, selectionDSL
				.Files().Exist()
				.Select(f => f.Path));
		}
		[Fact]
		public void Should_get_existent_files_for_revision()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch(1)
					.File("file1").Removed()
					.File("file2").Added()
					.File("file3").Added()
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file2").Removed()
					.File("file4").CopiedFrom("file2", "2")
			.Submit();

			Assert.Equal(new string[] { "file1" }, selectionDSL
				.Files().ExistInRevision("1")
				.Select(f => f.Path));
			Assert.Equal(new string[] { "file2", "file3" }, selectionDSL
				.Files().ExistInRevision("2")
				.Select(f => f.Path));
			Assert.Equal(new string[] { "file3", "file4" }, selectionDSL
				.Files().ExistInRevision("3")
				.Select(f => f.Path));
		}
		[Fact]
		public void Should_get_existent_files_on_different_branches()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0001)
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch(0011)
					.File("file1").Removed()
					.File("file2").Added()
			.Submit()
				.AddCommit("3").OnBranch(0101)
					.File("file2").Added()
					.File("file3").Added()
			.Submit();


			Assert.Equal(new string[] { "file1" }, selectionDSL
				.Files().ExistInRevision("1")
				.Select(f => f.Path));
			Assert.Equal(new string[] { "file2" }, selectionDSL
				.Files().ExistInRevision("2")
				.Select(f => f.Path));
			Assert.Equal(new string[] { "file1", "file2", "file3" }, selectionDSL
				.Files().ExistInRevision("3")
				.Select(f => f.Path));
		}
		[Fact]
		public void Should_get_files_in_directory()
		{
			mappingDSL
				.AddCommit("1")
					.File("/trunk/dir1/file1").Added()
			.Submit()
				.AddCommit("2")
					.File("/trunk/dir2changelog").Added()
					.File("/trunk/dir2/file2").Added()
			.Submit()
				.AddCommit("3")
					.File("/file3").Added()
			.Submit();

			Assert.Equal(1, selectionDSL
				.Files().InDirectory("/trunk/dir1").Count());
			Assert.Equal(1, selectionDSL
				.Files().InDirectory("/trunk/dir2").Count());
			Assert.Equal(3, selectionDSL
				.Files().InDirectory("/trunk").Count());
			Assert.Equal(4, selectionDSL
				.Files().InDirectory("/").Count());
		}
		[Fact]
		public void Should_get_defective_files()
		{
			mappingDSL
				.AddCommit("1").OnBranch(1)
					.File("file1").Added()
						.Code(100)
					.File("file2").Added()
						.Code(200)
			.Submit()
				.AddCommit("2").OnBranch(1).IsBugFix()
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(20)
			.Submit()
				.AddCommit("3").OnBranch(1)
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(20)
					.File("file2").Modified()
						.Code(-20)
						.Code(50)
					.File("file3").Added()
						.Code(300)
			.Submit()
				.AddCommit("4").OnBranch(1).IsBugFix()
					.File("file3").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("3")
						.Code(10)
			.Submit()
				.AddCommit("5").OnBranch(1).IsBugFix()
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("2")
						.Code(10)
			.Submit();

			Assert.Equal(new string[] { "file1" }, selectionDSL
				.Commits().TillNumber(2)
				.Modifications().InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles(null, null)
						.Select(x => x.Path));
			Assert.Equal(new string[] { "file1" }, selectionDSL
				.Commits()
					.TillNumber(2)
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles(null, "2")
						.Select(x => x.Path));
			Assert.Equal(new string[] { "file1" }, selectionDSL
				.Commits()
					.TillNumber(2)
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles("2", null)
						.Select(x => x.Path));
			Assert.Equal(new string[] {}, selectionDSL
				.Commits()
					.TillNumber(2)
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles("2", "4")
						.Select(x => x.Path));
			Assert.Equal(new string[] { "file3" }, selectionDSL
				.Commits()
					.TillNumber(3)
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles("3", "4")
						.Select(x => x.Path));
		}
	}
}
