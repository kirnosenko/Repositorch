using System;
using NSubstitute;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public class BaseMapperTest : BaseRepositoryTest
	{
		protected IVcsData vcsData;
		
		public BaseMapperTest()
		{
			vcsData = Substitute.For<IVcsData>();
		}
		protected ILog CreateLog(string revision, string author, DateTime date, string message)
		{
			var log = Substitute.For<ILog>();
			log.Revision.Returns(revision);
			log.Author.Returns(revision);
			log.Date.Returns(date);
			log.Message.Returns(message);
			return log;
		}
	}
}
