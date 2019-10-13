using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class ModificationMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_add_modification_for_added_file()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
			.Submit();

			Assert.Equal(1, Get<Modification>().Count());
			var m = Get<Modification>().Single();
			Assert.Equal(TouchedFileAction.ADDED, m.Action);
			Assert.Equal("1", m.Commit.Revision);
			Assert.Equal("file1", m.File.Path);
			Assert.Null(m.SourceCommit);
			Assert.Null(m.SourceFile);
		}
		[Fact]
		public void Should_add_modification_for_each_added_file()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
					.File("file2").Added()
			.Submit()
				.AddCommit("2")
					.File("file3").Added()
			.Submit();

			Assert.Equal(3, Get<Modification>().Count());
		}
		[Fact]
		public void Should_add_modification_for_modified_file()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
			.Submit()
				.AddCommit("3")
					.File("file1").Modified()
			.Submit();

			Assert.Equal(3, Get<Modification>().Count());
			var m = Get<Modification>().Last();
			Assert.Equal(TouchedFileAction.MODIFIED, m.Action);
			Assert.Equal("3", m.Commit.Revision);
			Assert.Equal("file1", m.File.Path);
			Assert.Null(m.SourceCommit);
			Assert.Null(m.SourceFile);
		}
		[Fact]
		public void Should_add_modification_for_copied_file()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Removed()
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("10").OnBranch("1")
					.File("file2").CopiedFrom("file1", "1")
					.File("file3").CopiedFrom("file1", "3")
			.Submit();

			Assert.Equal(5, Get<Modification>().Count());

			var m2 = Get<Modification>().Single(x => x.File.Path == "file2");
			Assert.Equal(TouchedFileAction.ADDED, m2.Action);
			Assert.Equal("10", m2.Commit.Revision);
			Assert.Equal("file2", m2.File.Path);
			Assert.Equal("1", m2.SourceCommit.Revision);
			Assert.Equal("file1", m2.SourceFile.Path);

			var m3 = Get<Modification>().Single(x => x.File.Path == "file3");
			Assert.Equal(TouchedFileAction.ADDED, m3.Action);
			Assert.Equal("10", m3.Commit.Revision);
			Assert.Equal("file3", m3.File.Path);
			Assert.Equal("3", m3.SourceCommit.Revision);
			Assert.Equal("file1", m3.SourceFile.Path);
		}
		[Fact]
		public void Should_add_modification_for_removed_file()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2")
					.File("file1").Removed()
			.Submit();

			Assert.Equal(2, Get<Modification>().Count());
			
			var m = Get<Modification>().Last();
			Assert.Equal(TouchedFileAction.REMOVED, m.Action);
			Assert.Equal("2", m.Commit.Revision);
			Assert.Equal("file1", m.File.Path);
			Assert.Null(m.CheckSum);
			Assert.Null(m.SourceCommit);
			Assert.Null(m.SourceFile);
		}
		[Fact]
		public void Should_allow_to_attach_checksum_for_each_modification()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added().HasCheckSum("sum1")
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified().HasCheckSum("sum2")
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file2").CopiedFrom("file1", "1").HasCheckSum("sum3")
			.Submit();

			Assert.Equal(3, Get<Modification>().Count());
			Assert.Equal(new string[] { "sum1", "sum2", "sum3" },
				Get<Modification>().Select(x => x.CheckSum));
		}
	}
}
