using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class CodeFileMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_add_file()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1")
			.Submit();

			Assert.Equal(1, Get<CodeFile>().Count());
			var f = Get<CodeFile>().Single();
			Assert.Equal("file1", f.Path);
			Assert.Equal("1", f.AddedInCommit.Revision);
			Assert.Null(f.DeletedInCommit);
		}
		[Fact]
		public void Should_add_several_files()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1")
					.AddFile("file2")
				.AddCommit("2")
					.AddFile("file3")
			.Submit();

			Assert.Equal(3, Get<CodeFile>().Count());
		}
		[Fact]
		public void Should_copy_file()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1")
			.Submit()
				.AddCommit("2")
					.File("file1").Delete()
			.Submit()
				.AddCommit("3")
					.AddFile("file1")
			.Submit()
				.AddCommit("10")
					.AddFile("file2").CopiedFrom("file1", "1")
					.AddFile("file3").CopiedFrom("file1", "3")
			.Submit();

			var f2 = Get<CodeFile>().Single(x => x.Path == "file2");
			Assert.Equal("1", f2.SourceCommit.Revision);
			Assert.Equal("file1", f2.SourceFile.Path);
			Assert.NotNull(f2.SourceFile.DeletedInCommitId);
			
			var f3 = Get<CodeFile>().Single(x => x.Path == "file3");
			Assert.Equal("3", f3.SourceCommit.Revision);
			Assert.Equal("file1", f3.SourceFile.Path);
			Assert.Null(f3.SourceFile.DeletedInCommitId);
		}
		[Fact]
		public void Should_mark_file_as_deleted()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1")
			.Submit()
				.AddCommit("2")
					.File("file1").Delete()
			.Submit();

			Assert.Equal(1, Get<CodeFile>().Count());
			var f = Get<CodeFile>().Single();
			Assert.Equal("2", f.DeletedInCommit.Revision);
		}
	}
}
