using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
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
			
			mapper.Map(
				mappingDSL.AddCommit("10").File("file2")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(TouchedFileAction.ADDED, modification.Action);
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
			
			mapper.Map(
				mappingDSL.AddCommit("10").File("file1")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file1", modification.File.Path);
			Assert.Equal(TouchedFileAction.MODIFIED, modification.Action);
			Assert.Null(modification.SourceCommit);
			Assert.Null(modification.SourceFile);
		}
		[Fact]
		public void Should_map_copied_file_with_source()
		{
			mappingDSL
				.AddCommit("5").OnBranch("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("6").OnBranch("1")
					.File("file1").Modified()
			.Submit();

			log.FileCopied("file2", "file1", "5");
			
			mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("1").File("file2")
			);
			SubmitChanges();

			Assert.Equal(3, Get<Modification>().Count());
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(TouchedFileAction.ADDED, modification.Action);
			Assert.Equal("5", modification.SourceCommit.Revision);
			Assert.Equal("file1", modification.SourceFile.Path);
		}
		[Fact]
		public void Should_set_previous_revision_on_the_same_branch_as_source_for_file_copied_without_source_revision()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Modified()
			.Submit()
				.AddCommit("4").OnBranch("11")
					.File("file1").Removed()
			.Submit();

			log.FileRenamed("file2", "file1");

			var modifications = mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("101").File("file2")
			);
			SubmitChanges();

			modifications.Count()
				.Should().Be(1);
			var modification = modifications.First().CurrentEntity<Modification>();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(TouchedFileAction.ADDED, modification.Action);
			Assert.Equal("3", modification.SourceCommit.Revision);
			Assert.Equal("file1", modification.SourceFile.Path);
		}
		[Fact]
		public void Should_map_removed_file()
		{
			mappingDSL
				.AddCommit("9")
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
			Assert.Equal(TouchedFileAction.REMOVED, modification.Action);
			Assert.Null(modification.SourceCommit);
			Assert.Null(modification.SourceFile);
			vcsData.DidNotReceiveWithAnyArgs()
				.Blame(null, null);
		}
		[Fact]
		public void Should_map_file_as_modified_in_merge()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file2").Modified()
			.Submit();

			log.ParentRevisionsAre("2", "3");
			
			var commitMappingExpression = mappingDSL.AddCommit("10").OnBranch("111");
			Assert.Equal(1, (int)mapper.Map(commitMappingExpression.File("file1")).Count());
			Assert.Equal(1, (int)mapper.Map(commitMappingExpression.File("file2")).Count());
		}
		[Fact]
		public void Should_map_fresh_file_as_added_in_merge()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
					.File("file2").Added()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Modified()
					.File("file3").Added()
			.Submit();

			log.ParentRevisionsAre("2", "3");
			log.FileAdded("file4");

			var commitMappingExpression = mappingDSL.AddCommit("10").OnBranch("111");
			var file1Exp = mapper.Map(commitMappingExpression.File("file4"));
			Assert.Equal(1, (int)file1Exp.Count());
			Assert.Equal(TouchedFileAction.ADDED, file1Exp.First().CurrentEntity<Modification>().Action);
		}
		[Fact]
		public void Should_map_file_as_added_or_removed_in_merge_when_it_has_different_history_state()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
					.File("file2").Removed()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Removed()
					.File("file2").Modified()
			.Submit();

			log.ParentRevisionsAre("2", "3");
			log.FileAdded("file1");
			log.FileRemoved("file2");
			
			var commitMappingExpression = mappingDSL.AddCommit("10").OnBranch("111");
			var file1Exp = mapper.Map(commitMappingExpression.File("file1"));
			Assert.Equal(1, (int)file1Exp.Count());
			Assert.Equal(TouchedFileAction.ADDED, file1Exp.First().CurrentEntity<Modification>().Action);
			var file2Exp = mapper.Map(commitMappingExpression.File("file2"));
			Assert.Equal(1, (int)file2Exp.Count());
			Assert.Equal(TouchedFileAction.REMOVED, file2Exp.First().CurrentEntity<Modification>().Action);
		}
		[Fact]
		public void Should_map_file_as_modified_in_merge_when_it_is_added_and_has_the_same_history_state()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file3").Added()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file4").Added()
			.Submit();

			log.ParentRevisionsAre("2", "3");
			log.FileAdded("file3");
			log.FileAdded("file4");

			var commitMappingExpression = mappingDSL.AddCommit("10").OnBranch("111");
			var file1Exp = mapper.Map(commitMappingExpression.File("file3"));
			Assert.Equal(1, (int)file1Exp.Count());
			Assert.Equal(TouchedFileAction.MODIFIED, file1Exp.First().CurrentEntity<Modification>().Action);
			var file2Exp = mapper.Map(commitMappingExpression.File("file4"));
			Assert.Equal(1, (int)file2Exp.Count());
			Assert.Equal(TouchedFileAction.MODIFIED, file2Exp.First().CurrentEntity<Modification>().Action);
		}

		[Fact]
		public void Should_map_file_as_modified_in_merge_when_it_is_added_and_has_modified_history_state()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file2").Added()
			.Submit()
				.AddCommit("3").OnBranch("11")
					.File("file2").Modified()
			.Submit()
				.AddCommit("4").OnBranch("101")
					.File("file3").Added()
			.Submit();

			log.ParentRevisionsAre("3", "4");
			log.FileAdded("file2");
			log.FileAdded("file3");

			var commitMappingExpression = mappingDSL.AddCommit("10").OnBranch("111");
			var file2Exp = mapper.Map(commitMappingExpression.File("file2"));
			Assert.Equal(1, (int)file2Exp.Count());
			Assert.Equal(TouchedFileAction.MODIFIED, file2Exp.First().CurrentEntity<Modification>().Action);
			var file3Exp = mapper.Map(commitMappingExpression.File("file3"));
			Assert.Equal(1, (int)file3Exp.Count());
			Assert.Equal(TouchedFileAction.MODIFIED, file3Exp.First().CurrentEntity<Modification>().Action);
		}
	}
}
