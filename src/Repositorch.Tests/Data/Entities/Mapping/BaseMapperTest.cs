using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public class BaseMapperTest : BaseRepositoryTest
	{
		protected class TestLog : Log
		{
			public TestLog(string revision, string author, DateTime date, string message, List<TouchedFile> touchedFiles = null)
			{
				Revision = revision;
				Author = author;
				Date = date;
				Message = message;
				this.touchedFiles = touchedFiles;
			}
		}

		protected IVcsData vcsData;
		
		public BaseMapperTest()
		{
			vcsData = Substitute.For<IVcsData>();
		}
	}
}
