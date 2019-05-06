using System;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public abstract class EntityMapper<T, IME, OME> : IMapper
	{
		protected IVcsData vcsData;

		public EntityMapper(IVcsData vcsData)
		{
			this.vcsData = vcsData;
		}
		public abstract IEnumerable<OME> Map(IME expression);
		public void RegisterHost(IMappingHost host)
		{
			host.RegisterMapper(this);
		}
		public Type Type
		{
			get { return typeof(T); }
		}
	}
}
