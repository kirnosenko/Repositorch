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
	}
}
