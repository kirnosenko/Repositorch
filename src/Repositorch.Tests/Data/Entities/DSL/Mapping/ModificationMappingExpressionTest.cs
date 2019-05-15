using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class ModificationMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_add_modification_linked_with_commit_and_file()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
					.AddFile("file2").Modified()
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
			.Submit();

			Assert.Equal(3, Get<Modification>().Count());
			Assert.Equal(new string[] { "1", "1", "2" },
				Get<Modification>().Select(m => m.Commit.Revision));
			Assert.Equal(new string[] { "file1", "file2", "file1" },
				Get<Modification>().Select(m => m.File.Path));
		}
	}
}
