using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;
using FluentAssertions;

namespace Repositorch.Data.Entities.Mapping
{
	public class CommitAttributeMapperTest : BaseMapperTest
	{
		private CommitAttributeMapper mapper;
		
		public CommitAttributeMapperTest()
		{
			mapper = new CommitAttributeMapper(vcsData);
		}
		
		[Fact]
		public void Should_mark_commit_as_merge()
		{
			vcsData.Log("10")
				.Returns(new TestLog("10").ParentRevisionsAre("1", "2"));

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Get<CommitAttribute>().Count()
				.Should().Be(1);
			var attribute = Get<CommitAttribute>().Single();
			attribute.Type
				.Should().Be(CommitAttribute.MERGE);
			attribute.Commit.Revision
				.Should().Be("10");
		}
		[Fact]
		public void Should_not_mark_commit_as_merge()
		{
			vcsData.Log("10")
				.Returns(new TestLog("10"));

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Get<CommitAttribute>().Count()
				.Should().Be(0);
		}

		[Fact]
		public void Should_mark_commit_as_split()
		{
			vcsData.Log("10")
				.Returns(new TestLog("10").ChildRevisionsAre("11", "12"));

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Get<CommitAttribute>().Count()
				.Should().Be(1);
			var attribute = Get<CommitAttribute>().Single();
			attribute.Type
				.Should().Be(CommitAttribute.SPLIT);
			attribute.Commit.Revision
				.Should().Be("10");
		}
		[Fact]
		public void Should_not_mark_commit_as_split()
		{
			vcsData.Log("10")
				.Returns(new TestLog("10"));

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Get<CommitAttribute>().Count()
				.Should().Be(0);
		}

		[Fact]
		public void Should_allow_to_get_all_attributes_from_expression()
		{
			vcsData.Log("10")
				.Returns(new TestLog("10")
					.ParentRevisionsAre("1", "2")
					.ChildRevisionsAre("11", "12"));

			var expressions = mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			expressions.Count()
				.Should().Be(1);
			var attributes = expressions.First().AllEntities<CommitAttribute>().ToArray();
			attributes.Length
				.Should().Be(2);
			attributes.Select(a => a.Type)
				.Should().BeEquivalentTo(new string[] {
					CommitAttribute.MERGE,
					CommitAttribute.SPLIT
				});
		}
	}
}
