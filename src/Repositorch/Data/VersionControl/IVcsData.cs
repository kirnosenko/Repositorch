using System;

namespace Repositorch.Data.VersionControl
{
	public interface IVcsData
	{
		string RevisionByNumber(int revisionNumber);
		string[] ParentRevisions(string revision);

		Log Log(string revision);
		IBlame Blame(string revision, string filePath);
	}
}
