﻿using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;

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

			var tags = Get<Tag>().ToArray();
			Assert.Equal(2, tags.Length);
			Assert.Equal(new string[] { "1.1", "fix" }, tags.Select(x => x.Title));
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

			Assert.Equal(0, Get<Tag>().Count());
		}
	}
}
