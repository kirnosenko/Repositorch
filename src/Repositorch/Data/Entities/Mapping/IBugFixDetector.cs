using System;

namespace Repositorch.Data.Entities.Mapping
{
	public interface IBugFixDetector
	{
		bool IsBugFix(Commit commit);
	}
}
