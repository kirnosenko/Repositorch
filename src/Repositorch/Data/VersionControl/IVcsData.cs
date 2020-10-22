using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	/// <summary>
	/// Abstraction to access data from version control system.
	/// </summary>
	public interface IVcsData
	{
		/// <summary>
		/// Get a revision by topological ordered number (count from 1).
		/// </summary>
		string GetRevisionByNumber(int number);
		/// <summary>
		/// Get the last revision on a considered branch.
		/// </summary>
		string GetLastRevision();
		/// <summary>
		/// Get all split (with multiple children) revisions untill specified one.
		/// </summary>
		IEnumerable<string> GetSplitRevisionsTillRevision(string revisionToStop);

		/// <summary>
		/// Get revision log.
		/// </summary>
		Log Log(string revision);
		/// <summary>
		/// Get blame for a specified file in a specified revision.
		/// </summary>
		IBlame Blame(string revision, string filePath);
	}
}
