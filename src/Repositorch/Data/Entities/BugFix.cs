using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Bug fix
	/// </summary>
	public class BugFix
	{
		public int Id { get; set; }

		/// <summary>
		/// Commit that clissified as a bugfix.
		/// </summary>
		public int CommitID { get; set; }
		public Commit Commit { get; set; }
	}
}
