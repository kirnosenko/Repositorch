using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Commit in VCS.
	/// </summary>
	public class Commit
	{
		public int Id { get; set; }
		/// <summary>
		/// Commit number in topologically ordered commit list.
		/// </summary>
		public int OrderedNumber { get; set; }
		/// <summary>
		/// Unique identifier of the commit in VCS.
		/// </summary>
		public string Revision { get; set; }
		/// <summary>
		/// Comments to the commit.
		/// </summary>
		public string Message { get; set; }
		/// <summary>
		/// The date the commit had taken place.
		/// </summary>
		public DateTime Date { get; set; }
		/// <summary>
		/// Author of the commit.
		/// </summary>
		public int AuthorId { get; set; }
		public Author Author { get; set; }

		//public int BranchId { get; set; }
		//public Branch Branch { get; set; }
	}
}
