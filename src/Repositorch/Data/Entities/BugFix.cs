using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Commit that clissified as a bugfix.
	/// </summary>
	public class BugFix
	{
		public int Id { get; set; }

		public int CommitID { get; set; }
		public Commit Commit { get; set; }
	}
}
