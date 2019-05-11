using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class AuthorMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_add_author()
		{
			mappingDSL
				.AddCommit("1")
					.AuthorIs("alan")
					.HasEmail("email")
			.Submit();

			Assert.Equal(1, Get<Author>().Count());
			var author = Get<Author>().Single();
			Assert.Equal("alan", author.Name);
			Assert.Equal("email", author.Email);
		}
		[Fact]
		public void Should_not_double_the_author()
		{
			mappingDSL
				.AddCommit("1")
					.AuthorIs("alan")
			.Submit()
				.AddCommit("2")
					.AuthorIs("alan")
			.Submit();

			Assert.Equal(1, Get<Author>().Count());
		}
	}
}
