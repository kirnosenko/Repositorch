using System;

namespace Repositorch.Data.Entities.Mapping
{
	public interface IMapper
	{
		void RegisterHost(IMappingHost host);
		Type Type { get; }
	}
}
