using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Modification of a file in a commit.
	/// </summary>
	public class Modification
	{
		public int Id { get; set; }
		/// <summary>
		/// Commit that contains modification.
		/// </summary>
		public int CommitID { get; set; }
		public Commit Commit { get; set; }
		/// <summary>
		/// File touched by modification.
		/// </summary>
		public int FileID { get; set; }
		public CodeFile File { get; set; }
	}
}
