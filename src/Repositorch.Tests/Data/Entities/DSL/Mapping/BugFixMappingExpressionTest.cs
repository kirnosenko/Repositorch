using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class BugFixMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_add_bugfix()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2").IsBugFix()
			.Submit()
				.AddCommit("3")
			.Submit();

			Assert.Equal(1, Get<BugFix>().Count());
			Assert.Equal("2", Get<BugFix>().Single().Commit.Revision);
		}
	}
}
