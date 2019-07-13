using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Piece of code that was added or removed.
	/// </summary>
	public class CodeBlock
	{
		public int Id { get; set; }
		/// <summary>
		/// Size of the code block.
		/// Can be measured in LOC or something else.
		/// A positive value means code addition.
		/// A negative value means code removing.
		/// </summary>
		public double Size { get; set; }
		/// <summary>
		/// Commit in which code was added in.
		/// Null for code removing.
		/// </summary>
		public int? AddedInitiallyInCommitId { get; set; }
		public Commit AddedInitiallyInCommit { get; set; }
		/// <summary>
		/// Modification code block was created in.
		/// </summary>
		public int ModificationId { get; set; }
		public Modification Modification { get; set; }
		/// <summary>
		/// A code block being changed by this one.
		/// Now code removing block keeps code block from which
		/// code being removed.
		/// Null for code addition block.
		/// </summary>
		public int? TargetCodeBlockId { get; set; }
		public CodeBlock TargetCodeBlock { get; set; }
	}
}
