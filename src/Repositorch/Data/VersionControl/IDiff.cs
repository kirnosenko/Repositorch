using System;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public interface IDiff
	{
		IEnumerable<string> TouchedFiles { get; }
	}
}
