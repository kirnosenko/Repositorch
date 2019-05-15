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
				.AddCommit("1")
					.AddFile("file1")
			.Submit()
				.AddCommit("2")
					.File("file1")
					.AddFile("file2")
			.Submit()
				.AddCommit("3")
					.AddFile("file3").CopiedFrom("file1", "1")
			.Submit();

			Assert.Equal(new string[] { "file1", "file2" }, selectionDSL
				.Commits().TillRevision("2")
				.Files().AddedInCommits()
				.Select(x => x.Path));
			Assert.Equal(new string[] { "file3" }, selectionDSL
				.Commits().FromRevision("3")
				.Files().AddedInCommits()
				.Select(x => x.Path));
		}
		[Fact]
		public void Should_get_files_deleted_in_specified_commits()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1")
			.Submit()
				.AddCommit("2")
					.File("file1").Delete()
					.AddFile("file2").CopiedFrom("file1", "1")
					.AddFile("file3")
			.Submit()
				.AddCommit("3")
					.File("file3").Delete()
			.Submit();

			Assert.Equal(new string[] { "file1" }, selectionDSL
				.Commits().TillRevision("2")
				.Files().DeletedInCommits()
				.Select(x => x.Path));
			Assert.Equal(new string[] { "file3" }, selectionDSL
				.Commits().FromRevision("3")
				.Files().DeletedInCommits()
				.Select(x => x.Path));
		}
		[Fact]
		public void Should_get_files_by_id()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
					.AddFile("file2").Modified()
			.Submit()
				.AddCommit("2")
					.File("file2").Modified()
					.AddFile("file3").Modified()
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
				.AddCommit("1").At(DateTime.Today.AddDays(-9))
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2").At(DateTime.Today.AddDays(-8))
					.AddFile("file2").Modified()
					.File("file1").Delete()
			.Submit()
				.AddCommit("3").At(DateTime.Today.AddDays(-7))
					.AddFile("file1").Modified()
			.Submit();

			Assert.Equal(2, selectionDSL
				.Files().PathIs("file1").Count());
			Assert.Equal(1, selectionDSL
				.Files().PathIs("file2").Count());
		}
		[Fact]
		public void Should_get_existent_files()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Delete()
					.AddFile("file2").Modified()
			.Submit();

			Assert.Equal(new string[] { "file2" }, selectionDSL
				.Files().Exist()
				.Select(f => f.Path));
		}
		[Fact]
		public void Should_get_existent_files_for_revision()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Delete()
					.AddFile("file2").Modified()
					.AddFile("file3").Modified()
			.Submit()
				.AddCommit("3")
					.File("file2").Delete()
					.AddFile("file4").CopiedFrom("file2", "2")
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
		public void Should_get_distinct_files()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
			.Submit();

			Assert.Equal(1, selectionDSL
				.Files().Count());
		}
		[Fact]
		public void Should_get_files_in_directory()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("/trunk/dir1/file1").Modified()
			.Submit()
				.AddCommit("2")
					.AddFile("/trunk/dir2changelog")
					.AddFile("/trunk/dir2/file2").Modified()
			.Submit()
				.AddCommit("3")
					.AddFile("/file3")
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
		/*
		[Fact]
		public void Should_get_defective_files()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
						.Code(100)
					.AddFile("file2").Modified()
						.Code(200)
			.Submit()
				.AddCommit("2").IsBugFix()
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(20)
			.Submit()
				.AddCommit("3")
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(20)
					.File("file2").Modified()
						.Code(-20)
						.Code(50)
					.AddFile("file3").Modified()
						.Code(300)
			.Submit()
				.AddCommit("4").IsBugFix()
					.File("file3").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("3")
						.Code(10)
			.Submit()
				.AddCommit("5").IsBugFix()
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("2")
						.Code(10)
			.Submit();
			
			selectionDSL
				.Commits()
					.TillRevision("2")
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles(null, null)
						.Select(x => x.Path).ToArray()
							.Should().Have.SameValuesAs(new string[]
							{
								"file1"
							});
			selectionDSL
				.Commits()
					.TillRevision("2")
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles(null, "2")
						.Select(x => x.Path).ToArray()
							.Should().Have.SameValuesAs(new string[]
							{
								"file1"
							});
			selectionDSL
				.Commits()
					.TillRevision("2")
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles("2", null)
						.Select(x => x.Path).ToArray()
							.Should().Have.SameValuesAs(new string[]
							{
								"file1"
							});
			selectionDSL
				.Commits()
					.TillRevision("2")
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles("2", "4")
						.Select(x => x.Path).ToArray()
							.Should().Be.Empty();
			selectionDSL
				.Commits()
					.TillRevision("3")
				.Modifications()
					.InCommits()
				.CodeBlocks()
					.InModifications().DefectiveFiles("3", "4")
						.Select(x => x.Path).ToArray()
							.Should().Have.SameSequenceAs(new string[]
							{
								"file3"
							});
		}*/
	}
}
