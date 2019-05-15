using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class ModificationSelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_select_modifications_for_commits()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
					.AddFile("file2").Modified()
			.Submit();

			Assert.Equal(1, selectionDSL
				.Commits().RevisionIs("1")
				.Modifications().InCommits()
				.Count());
			Assert.Equal(2, selectionDSL
				.Commits().RevisionIs("2")
				.Modifications().InCommits()
				.Count());
			Assert.Equal(3, selectionDSL
				.Modifications().InCommits()
				.Count());
		}
		[Fact]
		public void Should_select_modifications_for_files()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
					.AddFile("file2").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
					.AddFile("file3").Modified()
			.Submit()
				.AddCommit("3")
					.File("file1").Modified()
					.File("file2").Modified()
			.Submit();

			Assert.Equal(3, selectionDSL
				.Files().PathIs("file1")
				.Modifications().InFiles()
				.Count());
			Assert.Equal(2, selectionDSL
				.Files().PathIs("file2")
				.Modifications().InFiles()
				.Count());
			Assert.Equal(1, selectionDSL
				.Files().PathIs("file3")
				.Modifications().InFiles()
				.Count());
		}
		[Fact]
		public void Should_select_unique_commits_that_contain_modifications()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
					.AddFile("file2").Modified()
			.Submit();

			Assert.Equal(new string[] { "1", "2" }, selectionDSL
				.Files().PathIs("file1")
				.Modifications().InFiles()
				.Commits().ContainModifications()
				.Select(x => x.Revision));
			Assert.Equal(new string[] { "2" }, selectionDSL
				.Files().PathIs("file2")
				.Modifications().InFiles()
				.Commits().ContainModifications()
				.Select(x => x.Revision));
			Assert.Equal(new string[] { "1", "2" }, selectionDSL
				.Commits().ContainModifications()
				.Select(x => x.Revision));
		}
		[Fact]
		public void Should_select_unique_files_that_contain_modifications()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
					.AddFile("file2").Modified()
			.Submit();

			Assert.Equal(new string[] { "file1", "file2" }, selectionDSL
				.Files().ContainModifications()
				.Select(x => x.Path));
		}
		[Fact]
		public void Should_get_unique_files_touched_in_specified_commits()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
					.AddFile("file2").Modified()
			.Submit();

			Assert.Equal(new string[] { "file1" }, selectionDSL
				.Commits().Reselect(s => s.Where(c => c.Revision == "1"))
				.Files().TouchedInCommits()
				.Select(f => f.Path));
			Assert.Equal(new string[] { "file1", "file2" }, selectionDSL
				.Commits().Reselect(s => s.Where(c => c.Revision == "2"))
				.Files().TouchedInCommits()
				.Select(f => f.Path));
			Assert.Equal(new string[] { "file1", "file2" }, selectionDSL
				.Files().TouchedInCommits()
				.Select(f => f.Path));
		}
		[Fact]
		public void Should_get_unique_commits_touch_specified_files()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
					.AddFile("file2").Modified()
			.Submit();

			Assert.Equal(new string[] { "1", "2" }, selectionDSL
				.Files().PathIs("file1")
				.Commits().TouchFiles()
				.Select(c => c.Revision));
			Assert.Equal(new string[] { "2" }, selectionDSL
				.Files().PathIs("file2")
				.Commits().TouchFiles()
				.Select(c => c.Revision));
			Assert.Equal(new string[] { "1", "2" }, selectionDSL
				.Files()
				.Commits().TouchFiles()
				.Select(c => c.Revision));
		}
	}
}
