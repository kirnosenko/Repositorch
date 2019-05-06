using System;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public interface ILog
	{
		string Revision { get; }
		string Author { get; }
		DateTime Date { get; }
		string Message { get; }
	}
}
