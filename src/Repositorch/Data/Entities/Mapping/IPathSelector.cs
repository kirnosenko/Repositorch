using System;

namespace Repositorch.Data.Entities.Mapping
{
	public interface IPathSelector
	{
		bool IsSelected(string path);
	}
}
