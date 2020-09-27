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
		/// May be measured in LOC or something else.
		/// A positive value means real code addition 
		/// or cancel of removing (not the same thing).
		/// A negative value means real code removing 
		/// or cancel of addition (the same thing).
		/// </summary>
		public double Size { get; set; }
		/// <summary>
		/// Commit in which code was added in.
		/// Not null for code addition.
		/// </summary>
		public int? AddedInitiallyInCommitNumber { get; set; }
		public Commit AddedInitiallyInCommit { get; set; }
		/// <summary>
		/// Modification code block was created in.
		/// </summary>
		public int ModificationId { get; set; }
		public Modification Modification { get; set; }
		/// <summary>
		/// A code block (addition) being changed by this one.
		/// Null for real code addition.
		/// Not null for code removing or cancel of removing.
		/// </summary>
		public int? TargetCodeBlockId { get; set; }
		public CodeBlock TargetCodeBlock { get; set; }
	}
}
