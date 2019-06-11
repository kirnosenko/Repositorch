using System;
using System.Collections.Generic;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Branch the commit is sitting on.
	/// Not a branch in terms of git but a separate line of code evolution.
	/// </summary>
	public class Branch
	{
		/// <summary>
		/// Size of mask in bits.
		/// </summary>
		public const int MaskSize = 32;

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
