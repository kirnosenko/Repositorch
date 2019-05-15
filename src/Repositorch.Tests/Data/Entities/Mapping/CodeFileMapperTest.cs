using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeFileMapperTest : BaseMapperTest
	{
		private CodeFileMapper mapper;
		private List<TouchedFile> touchedFiles;

		public CodeFileMapperTest()
		{
			touchedFiles = new List<TouchedFile>();
			vcsData
				.Log(Arg.Is<string>("10"))
				.Returns(new TestLog("10", "alan", DateTime.Today, "text", touchedFiles));
			vcsData.ParentRevisions(Arg.Is<string>("10"))
				.Returns(new string[] { "9" });
			mapper = new CodeFileMapper(vcsData);
		}
		[Fact]
		public void Should_map_added_file()
		{
			AddFile("file1");

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeFile>().Count());
			var f = Get<CodeFile>().Single();
			Assert.Equal("file1", f.Path);
			Assert.Equal("10", f.AddedInCommit.Revision);
		}
		[Fact]
		public void Should_not_map_anything_for_modified_file()
		{
			mappingDSL
				.AddCommit("9").At(DateTime.Today.AddDays(-1))
					.AddFile("file1")
			.Submit();

			ModifyFile("file1");

			CodeFile file = 
			mapper.Map(
				mappingDSL.AddCommit("10")
			).Single().CurrentEntity<CodeFile>();
			SubmitChanges();

			Assert.Equal(1, Get<CodeFile>().Count());
			Assert.Equal("9", file.AddedInCommit.Revision);
		}
		[Fact]
		public void Should_map_copied_file_with_source()
		{
			mappingDSL
				.AddCommit("9").At(DateTime.Today.AddDays(-1))
					.AddFile("file1")
			.Submit();

			CopyFile("file2", "file1", "9");
			
			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal(2, Get<CodeFile>().Count());
			var f = Get<CodeFile>().Single(x => x.Path == "file2");
			Assert.Equal("file1", f.SourceFile.Path);
			Assert.Equal("9", f.SourceCommit.Revision);
		}
		[Fact]
		public void Should_set_previous_revision_as_source_for_file_copied_without_source_revision()
		{
			mappingDSL
				.AddCommit("9").At(DateTime.Today.AddDays(-1))
					.AddFile("file1")
			.Submit();

			RenameFile("file2", "file1");
				
			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			var f = Get<CodeFile>().Single(x => x.Path == "file2");
			Assert.Equal("file1", f.SourceFile.Path);
			Assert.Equal("9", f.SourceCommit.Revision);
		}
		[Fact]
		public void Should_map_deleted_file()
		{
			mappingDSL
				.AddCommit("9").At(DateTime.Today.AddDays(-1))
					.AddFile("file1")
			.Submit();

			DeleteFile("file1");

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal("10", Get<CodeFile>().Single().DeletedInCommit.Revision);
		}
		[Fact]
		public void Should_use_path_selectors()
		{
			AddFile("file1.123");
			AddFile("file2.555");
			AddFile("file3.123");
			AddFile("file4.555");

			var selector = Substitute.For<IPathSelector>();
			selector
				.IsSelected(Arg.Any<string>())
				.Returns(x => (x[0] as string).EndsWith(".555"));
			mapper.PathSelectors = new IPathSelector[] { selector };

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal(new string[] { "file2.555", "file4.555" }, Get<CodeFile>()
				.Select(x => x.Path));
		}
		[Fact]
		public void Should_use_all_path_selectors()
		{
			AddFile("/dir1/file1.123");
			AddFile("/dir1/file2.555");
			AddFile("/dir2/file3.555");

			var selector1 = Substitute.For<IPathSelector>();
			selector1
				.IsSelected(Arg.Any<string>())
				.Returns(x => (x[0] as string).EndsWith(".555"));
			var selector2 = Substitute.For<IPathSelector>();
			selector2
				.IsSelected(Arg.Any<string>())
				.Returns(x => (x[0] as string).StartsWith("/dir1"));
			mapper.PathSelectors = new IPathSelector[] { selector1, selector2 };
			
			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeFile>().Count());
			Assert.Equal("/dir1/file2.555", Get<CodeFile>()
				.Single().Path);
		}
		[Fact]
		public void Should_map_ranamed_file_during_partial_mapping()
		{
			mappingDSL
				.AddCommit("9")
					.AddFile("file1.c").Modified()
			.Submit()
				.AddCommit("10")
					.File("file1.c").Delete()
			.Submit()
				.AddCommit("11")
					.AddFile("file3.c").Modified()
			.Submit();
			
			RenameFile("file1.cpp", "file1.c");

			var selector = Substitute.For<IPathSelector>();
			selector
				.IsSelected(Arg.Any<string>())
				.Returns(x => (x[0] as string).EndsWith(".cpp"));
			mapper.PathSelectors = new IPathSelector[] { selector };

			mapper.Map(
				mappingDSL.Commit("10")
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeFile>()
				.Where(x => x.Path == "file1.cpp").Count());
		}
		
		private void AddFile(string path)
		{
			TouchPath(path, TouchedFile.TouchedFileAction.ADDED, null, null);
		}
		private void ModifyFile(string path)
		{
			TouchPath(path, TouchedFile.TouchedFileAction.MODIFIED, null, null);
		}
		private void CopyFile(string path, string sourcePath, string sourceRevision)
		{
			TouchPath(path, TouchedFile.TouchedFileAction.ADDED, sourcePath, sourceRevision);
		}
		private void RenameFile(string path, string sourcePath)
		{
			DeleteFile(sourcePath);
			CopyFile(path, sourcePath, null);
		}
		private void DeleteFile(string path)
		{
			TouchPath(path, TouchedFile.TouchedFileAction.DELETED, null, null);
		}
		private void TouchPath(string path, TouchedFile.TouchedFileAction action, string sourcePath, string sourceRevision)
		{
			touchedFiles.Add(new TouchedFile()
			{
				Path = path,
				Action = action,
				SourcePath = sourcePath,
				SourceRevision = sourceRevision
			});
		}
	}
}
