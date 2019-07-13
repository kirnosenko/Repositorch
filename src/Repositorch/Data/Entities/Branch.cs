using System;
using System.Collections.Generic;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Branch the commit is sitting on.
	/// Not a branch in terms of git but a separate line of code evolution
	/// on commit graph of single git branch (master or something else).
	/// </summary>
	public class Branch
	{
		/// <summary>
		/// Size of mask in bits.
		/// </summary>
		public const int MaskSize = 32;
		/// <summary>
		/// Max value of mask.
		/// </summary>
		public const uint MaskMax = uint.MaxValue;

		public int Id { get; set; }
		/// <summary>
		/// Mask to distinguish parent branches.
		/// </summary>
		public uint Mask { get; set; }
		/// <summary>
		/// Offset of the mask in bits.
		/// </summary>
		public uint MaskOffset { get; set; }

		public List<Commit> Commits { get; set; } = new List<Commit>();
	}
}
