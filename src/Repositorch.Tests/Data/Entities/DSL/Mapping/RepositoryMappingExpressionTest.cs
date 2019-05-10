using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class RepositoryMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_not_keep_expression_chain_after_submit()
		{
			Assert.NotEqual(
				mappingDSL,
				mappingDSL
					.AddCommit("1").Submit());
		}
		[Fact]
		public void Can_give_last_entity_by_type()
		{
			var exp = mappingDSL
				.AddCommit("1");
			//.AddFile("file1").Modified()
			//.AddFile("file2").Modified();

			Assert.Equal("1", exp.CurrentEntity<Commit>().Revision);
			Assert.Null(exp.CurrentEntity<BugFix>());
			//exp.CurrentEntity<ProjectFile>().Path
			//	.Should().Be("file2");
		}
	}
}
