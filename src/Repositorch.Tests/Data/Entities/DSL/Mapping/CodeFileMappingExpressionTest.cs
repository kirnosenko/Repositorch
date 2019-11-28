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
					.File("file1").Added()
			.Submit();

			Assert.Equal(1, Get<CodeFile>().Count());
			Assert.Equal("file1", Get<CodeFile>().Single().Path);
		}
		[Fact]
		public void Should_use_the_same_file_for_all_path_reincarnations()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
					.File("file2").Added()
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file1").Removed()
					.File("file2").Modified()
			.Submit()
				.AddCommit("4").OnBranch("1")
					.File("file1").CopiedFrom("file2", "3")
					.File("file2").Removed()
			.Submit();

			Assert.Equal(2, Get<CodeFile>().Count());
			Assert.Equal(new string[] { "file1", "file2" }, Get<CodeFile>()
				.Select(f => f.Path));
		}
	}
}
