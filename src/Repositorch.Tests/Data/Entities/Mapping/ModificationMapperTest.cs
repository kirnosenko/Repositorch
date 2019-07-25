using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class ModificationMapperTest : BaseMapperTest
	{
		private ModificationMapper mapper;
		private TestLog log;
		
		public ModificationMapperTest()
		{
			log = new TestLog("10", null, null, DateTime.Today, null);
			vcsData
				.Log(Arg.Is<string>("10"))
				.Returns(log);
			mapper = new ModificationMapper(vcsData);
		}
		[Fact]
		public void Should_map_modifacation_for_added_file()
		{
			mappingDSL
				.AddCommit("9")
					.File("file1").Added()
			.Submit();

			log.FileAdded("file2");

			CommitMappingExpression commitExp = mappingDSL.AddCommit("10");
			mapper.Map(
				commitExp.File("file2")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());
			
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(Modification.FileAction.ADDED, modification.Action);
			Assert.Null(modification.SourceCommit);
			Assert.Null(modification.SourceFile);
		}
		[Fact]
		public void Should_map_modifacation_for_modified_file()
		{
			mappingDSL
				.AddCommit("9")
					.File("file1").Added()
			.Submit();

			log.FileModified("file1");

			CommitMappingExpression commitExp = mappingDSL.AddCommit("10");
			mapper.Map(
				commitExp.File("file1")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());

			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file1", modification.File.Path);
			Assert.Equal(Modification.FileAction.MODIFIED, modification.Action);
			Assert.Null(modification.SourceCommit);
			Assert.Null(modification.SourceFile);
		}
		[Fact]
		public void Should_map_copied_file_with_source()
		{
			mappingDSL
				.AddCommit("9").OnBranch(1)
					.File("file1").Added()
			.Submit();

			log.FileCopied("file2", "file1", "9");

			mapper.Map(
				mappingDSL.AddCommit("10").OnBranch(1).File("file2")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());

			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(Modification.FileAction.ADDED, modification.Action);
			Assert.Equal("9", modification.SourceCommit.Revision);
			Assert.Equal("file1", modification.SourceFile.Path);
		}
		[Fact]
		public void Should_set_previous_revision_as_source_for_file_copied_without_source_revision()
		{
			mappingDSL
				.AddCommit("9").OnBranch(1)
					.File("file1").Added()
			.Submit();

			log.FileRenamed("file2", "file1");

			mapper.Map(
				mappingDSL.AddCommit("10").OnBranch(1).File("file2")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());

			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(Modification.FileAction.ADDED, modification.Action);
			Assert.Equal("9", modification.SourceCommit.Revision);
			Assert.Equal("file1", modification.SourceFile.Path);
		}
		[Fact]
		public void Should_map_removed_file()
		{
			mappingDSL
				.AddCommit("9").At(DateTime.Today.AddDays(-1))
					.File("file1").Added()
			.Submit();

			log.FileRemoved("file1");

			mapper.Map(
				mappingDSL.AddCommit("10").File("file1")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());

			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file1", modification.File.Path);
			Assert.Equal(Modification.FileAction.REMOVED, modification.Action);
			Assert.Null(modification.SourceCommit);
			Assert.Null(modification.SourceFile);
		}
	}
}
