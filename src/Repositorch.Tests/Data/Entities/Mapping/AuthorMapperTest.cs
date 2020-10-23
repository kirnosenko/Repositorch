using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public class AuthorMapperTest : BaseMapperTest
	{
		private AuthorMapper mapper;

		public AuthorMapperTest()
		{
			mapper = new AuthorMapper(vcsData);
		}
		[Fact]
		public void Should_add_author()
		{
			var log = new TestLog("1", "Ivan", "ivan@mail.com", DateTime.Today, "none");
			vcsData.Log(Arg.Is<string>("1")).Returns(log);

			mapper.Map(
				mappingDSL.AddCommit("1")
			);
			SubmitChanges();
			
			Assert.Equal(1, Get<Author>().Count());
			var author = Get<Author>().Single();
			Assert.Equal(log.AuthorName, author.Name);
			Assert.Equal(log.AuthorEmail, author.Email);
		}
	}
}
