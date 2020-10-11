using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class TagMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_mark_commit_with_tags()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2").HasTag("1.0")
			.Submit()
				.AddCommit("3").HasTags("1.1", "fix")
			.Submit();

			var tags = Get<CommitAttribute>().ToArray();
			Assert.Equal(3, tags.Length);
			tags.Select(x => x.Data)
				.Should().BeEquivalentTo(new string[] { "1.0", "1.1", "fix" });
			tags.Select(x => x.Commit.Revision)
				.Should().BeEquivalentTo(new string[] { "2", "3", "3" });
		}
	}
}
