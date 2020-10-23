using System;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public class TestBlame : Dictionary<string, double>, IBlame
	{
		public TestBlame AddLinesFromRevision(string revision, int lines)
		{
			this[revision] = lines;
			return this;
		}
	}
}
