using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// File with code under control of VCS.
	/// </summary>
	public class CodeFile
	{
		public int Id { get; set; }
		/// <summary>
		/// UNIX-formated path of the file with leading slash.
		/// </summary>
		public string Path { get; set; }
		/// <summary>
		/// Commit the file was added in.
		/// </summary>
		public int AddedInCommitID { get; set; }
		public Commit AddedInCommit { get; set; }
		/// <summary>
		/// Commit the file was deleted in.
		/// Null if the file exists so far.
		/// </summary>
		public int? DeletedInCommitID { get; set; }
		public Commit DeletedInCommit { get; set; }
	}
}
