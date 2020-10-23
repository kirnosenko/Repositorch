using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public class TagMapperTest : BaseMapperTest
	{
		private TagMapper mapper;
		
		public TagMapperTest()
		{
			mapper = new TagMapper(vcsData);
		}

		[Fact]
		public void Should_add_tags_for_commit_with_tags()
		{
			vcsData.Log("1")
				.Returns(new TestLog("1").TagsAre(new string[] { "1.1", "fix" }));

			mapper.Map(
				mappingDSL.AddCommit("1")
			);
			SubmitChanges();

			var tags = Get<CommitAttribute>().ToArray();
			tags.Length
				.Should().Be(2);
			tags.Select(t => t.Data)
				.Should().BeEquivalentTo(new string[] { "1.1", "fix" });
			tags.All(t => t.Commit.Revision == "1")
				.Should().BeTrue();
		}
		[Fact]
		public void Should_not_add_tag_for_commit_without_tags()
		{
			vcsData.Log("1")
				.Returns(new TestLog("1"));

			mapper.Map(
				mappingDSL.AddCommit("1")
			);
			SubmitChanges();

			Get<CommitAttribute>().Count()
				.Should().Be(0);
		}
	}
}
