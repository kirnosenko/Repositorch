using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Piece of code that was added or removed.
	/// </summary>
	public class CodeBlock
	{
		public int ID { get; set; }
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
		public int? AddedInitiallyInCommitID { get; set; }
		public Commit AddedInitiallyInCommit { get; set; }
		/// <summary>
		/// Modification code block was created in.
		/// Null for code removing.
		/// </summary>
		public int ModificationID { get; set; }
		public Modification Modification { get; set; }
		/// <summary>
		/// A code block being changed by this one.
		/// Now code removing block keeps code block from which
		/// code being removed.
		/// </summary>
		public int? TargetCodeBlockID { get; set; }
		public CodeBlock TargetCodeBlock { get; set; }
	}
}
