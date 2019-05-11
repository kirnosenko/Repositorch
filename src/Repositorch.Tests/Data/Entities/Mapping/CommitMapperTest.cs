using System;
using System.Linq;
using Xunit;
using NSubstitute;

namespace Repositorch.Data.Entities.Mapping
{
	public class CommitMapperTest : BaseMapperTest
	{
		private CommitMapper mapper;

		public CommitMapperTest()
		{
			mapper = new CommitMapper(vcsData);
		}
		[Fact]
		public void Should_add_commit()
		{
			var log = new TestLog("1", "Ivan", DateTime.Today, "none");
			vcsData.Log(Arg.Is<string>("1")).Returns(log);
			
			mappingDSL.Revision = "1";
			mapper.Map(mappingDSL);
			SubmitChanges();

			Assert.Equal(1, Get<Commit>().Count());
			var commit = Get<Commit>().Single();
			Assert.Equal(log.Revision, commit.Revision);
			Assert.Equal(log.Date, commit.Date);
			Assert.Equal(log.Message, commit.Message);
		}
	}
}
