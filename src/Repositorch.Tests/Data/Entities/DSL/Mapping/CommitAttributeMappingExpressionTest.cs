using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class CommitAttributeMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_mark_commit_as_merge()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3").IsMerge()
			.Submit();

			Get<CommitAttribute>().Count()
				.Should().Be(1);
			var merge = Get<CommitAttribute>().Single();
			merge.Type
				.Should().Be(CommitAttribute.MERGE);
			merge.Data
				.Should().BeNull();
			merge.Commit.Revision
				.Should().Be("3");
		}
		[Fact]
		public void Should_mark_commit_as_split()
		{
			mappingDSL
				.AddCommit("1").IsSplit()
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit();

			Get<CommitAttribute>().Count()
				.Should().Be(1);
			var split = Get<CommitAttribute>().Single();
			split.Type
				.Should().Be(CommitAttribute.SPLIT);
			split.Data
				.Should().BeNull();
			split.Commit.Revision
				.Should().Be("1");
		}
		[Fact]
		public void Should_mark_commit_with_several_attributes()
		{
			mappingDSL
				.AddCommit("1").IsMerge().IsSplit().IsBugFix().HasTag("1.0")
			.Submit();

			var attributes = Get<CommitAttribute>().ToArray();
			attributes.Select(a => a.Type)
				.Should().BeEquivalentTo(new string[]
				{
					CommitAttribute.MERGE,
					CommitAttribute.SPLIT,
					CommitAttribute.FIX,
					CommitAttribute.TAG
				});
			attributes.All(a => a.Commit.Revision == "1")
				.Should().BeTrue();
		}
	}
}
