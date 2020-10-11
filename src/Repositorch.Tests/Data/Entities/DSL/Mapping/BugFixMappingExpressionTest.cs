using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class BugFixMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_mark_commit_as_bugfix()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2").IsBugFix()
			.Submit()
				.AddCommit("3")
			.Submit();

			Get<CommitAttribute>().Count()
				.Should().Be(1);
			var attribute = Get<CommitAttribute>().Single();
			attribute.Type
				.Should().Be(CommitAttribute.FIX);
			attribute.Data
				.Should().BeNull();
			attribute.Commit.Revision
				.Should().Be("2");
		}
	}
}
