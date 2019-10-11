using System;
using System.Collections.Generic;
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
		private Dictionary<string,TestBlame> blames;
		
		public ModificationMapperTest()
		{
			log = new TestLog("10", null, null, DateTime.Today, null);
			vcsData
				.Log(Arg.Is<string>("10"))
				.Returns(log);
			blames = new Dictionary<string, TestBlame>();
			vcsData
				.Blame(Arg.Is<string>("10"), Arg.Any<string>())
				.Returns(x => blames[(string)x[1]]);
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
			blames.Add("file2", new TestBlame() { CheckSum = "sha2" });

			mapper.Map(
				mappingDSL.AddCommit("10").File("file2")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(Modification.FileAction.ADDED, modification.Action);
			Assert.Equal("sha2", modification.CheckSum);
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
			blames.Add("file1", new TestBlame() { CheckSum = "sha1" });

			mapper.Map(
				mappingDSL.AddCommit("10").File("file1")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file1", modification.File.Path);
			Assert.Equal(Modification.FileAction.MODIFIED, modification.Action);
			Assert.Equal("sha1", modification.CheckSum);
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
					.File("file1").Modified();

			log.FileCopied("file2", "file1", "5");
			blames.Add("file2", new TestBlame() { CheckSum = "sha2" });

			mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("1").File("file2")
			);
			SubmitChanges();

			Assert.Equal(3, Get<Modification>().Count());
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(Modification.FileAction.ADDED, modification.Action);
			Assert.Equal("sha2", modification.CheckSum);
			Assert.Equal("5", modification.SourceCommit.Revision);
			Assert.Equal("file1", modification.SourceFile.Path);
		}
		[Fact]
		public void Should_set_previous_revision_as_source_for_file_copied_without_source_revision()
		{
			mappingDSL
				.AddCommit("5").OnBranch("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("6").OnBranch("1")
					.File("file1").Modified()
			.Submit();
					
			log.FileRenamed("file2", "file1");
			blames.Add("file2", new TestBlame() { CheckSum = "sha2" });

			mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("1").File("file2")
			);
			SubmitChanges();

			Assert.Equal(3, Get<Modification>().Count());
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file2", modification.File.Path);
			Assert.Equal(Modification.FileAction.ADDED, modification.Action);
			Assert.Equal("sha2", modification.CheckSum);
			Assert.Equal("6", modification.SourceCommit.Revision);
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
			blames.Add("file1", new TestBlame() { CheckSum = "sha1" });

			mapper.Map(
				mappingDSL.AddCommit("10").File("file1")
			);
			SubmitChanges();

			Assert.Equal(2, Get<Modification>().Count());
			var modification = Get<Modification>().Last();
			Assert.Equal("10", modification.Commit.Revision);
			Assert.Equal("file1", modification.File.Path);
			Assert.Equal(Modification.FileAction.REMOVED, modification.Action);
			Assert.Null(modification.CheckSum);
			Assert.Null(modification.SourceCommit);
			Assert.Null(modification.SourceFile);
			vcsData.DidNotReceiveWithAnyArgs()
				.Blame(null, null);
		}
		[Fact]
		public void Should_map_file_as_modified_in_merge_when_checksum_is_different()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added().HasCheckSum("sha1")
					.File("file2").Added().HasCheckSum("sha2")
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified().HasCheckSum("sha11")
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file2").Modified().HasCheckSum("sha22")
			.Submit();

			blames.Add("file1", new TestBlame() { CheckSum = "sha11" });
			blames.Add("file2", new TestBlame() { CheckSum = "sha222" });

			var commitMappingExpression = mappingDSL.AddCommit("10").OnBranch("111");
			Assert.Equal(0, (int)mapper.Map(commitMappingExpression.File("file1")).Count());
			Assert.Equal(1, (int)mapper.Map(commitMappingExpression.File("file2")).Count());
		}
	}
}
