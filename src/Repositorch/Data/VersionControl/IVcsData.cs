using System;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public interface IVcsData
	{
		string GetRevisionByNumber(int number);
		IEnumerable<string> GetRevisionParents(string revision);
		IEnumerable<string> GetRevisionChildren(string revision);
		
		Log Log(string revision);
		IBlame Blame(string revision, string filePath);
	}
}
