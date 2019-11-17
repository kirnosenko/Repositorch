using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Tag of commit.
	/// </summary>
	public class Tag
	{
		public int Id { get; set; }
		/// <summary>
		/// Tag title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The tagged commit.
		/// </summary>
		public int CommitId { get; set; }
		public Commit Commit { get; set; }
	}
}
