using System;

namespace Repositorch.Data.VersionControl
{
	public interface IVcsData
	{
		string GetRevisionByNumber(int number);

		Log Log(string revision);
		IBlame Blame(string revision, string filePath);
	}
}
