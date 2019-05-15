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
		public int AddedInCommitId { get; set; }
		public Commit AddedInCommit { get; set; }
		/// <summary>
		/// Commit the file was deleted in.
		/// Null if the file exists so far.
		/// </summary>
		public int? DeletedInCommitId { get; set; }
		public Commit DeletedInCommit { get; set; }
		/// <summary>
		/// The source file of the file.
		/// Null if the file was created from scratch.
		/// </summary>
		public int? SourceFileId { get; set; }
		public CodeFile SourceFile { get; set; }
		/// <summary>
		/// The revision the source file was taken from.
		/// </summary>
		public int? SourceCommitId { get; set; }
		public Commit SourceCommit { get; set; }
	}
}
