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
		/// Type of the modification.
		/// </summary>
		public TouchedFileAction Action { get; set; }
		/// <summary>
		/// Commit that contains modification.
		/// </summary>
		public int CommitNumber { get; set; }
		public Commit Commit { get; set; }
		/// <summary>
		/// File touched by the modification.
		/// </summary>
		public int FileId { get; set; }
		public CodeFile File { get; set; }
		/// <summary>
		/// The source file of a new file.
		/// Null if the file was created from scratch.
		/// </summary>
		public int? SourceFileId { get; set; }
		public CodeFile SourceFile { get; set; }
		/// <summary>
		/// The revision the source file was taken from.
		/// </summary>
		public int? SourceCommitNumber { get; set; }
		public Commit SourceCommit { get; set; }
	}
}
