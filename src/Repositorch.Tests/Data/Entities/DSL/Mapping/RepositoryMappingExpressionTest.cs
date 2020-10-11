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
				.AddCommit("1")
					.File("file1").Added()
					.File("file2").Added();

			Assert.Equal("1", exp.CurrentEntity<Commit>().Revision);
			Assert.Null(exp.CurrentEntity<CommitAttribute>());
			Assert.Equal("file2", exp.CurrentEntity<CodeFile>().Path);
		}
	}
}
