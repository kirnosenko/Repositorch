using System;
using System.Collections.Generic;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public abstract class EntityMapper<IME, OME>
		where IME : IRepositoryMappingExpression
		where OME : IRepositoryMappingExpression
	{
		protected IVcsData vcsData;

		public EntityMapper(IVcsData vcsData)
		{
			this.vcsData = vcsData;
		}
		public abstract IEnumerable<OME> Map(IME expression);
	}
}
