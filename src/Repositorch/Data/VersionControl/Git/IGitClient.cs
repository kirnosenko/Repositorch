using System;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	public interface IGitClient
	{
		/// <summary>
		/// Get list of all revisions in reverse topological order
		/// (i.e. descendant commits are after their parents).
		/// </summary>
		/// <returns>Resulting stream.</returns>
		Stream RevList();
		Stream Log(string revision);
		Stream Diff(string revision);
		Stream Blame(string revision, string filePath);
	}
}
