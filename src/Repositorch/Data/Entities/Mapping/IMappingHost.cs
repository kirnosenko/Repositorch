using System;

namespace Repositorch.Data.Entities.Mapping
{
	public interface IMappingHost
	{
		void RegisterMapper<T, IME, OME>(EntityMapper<T, IME, OME> mapper);
	}
}
