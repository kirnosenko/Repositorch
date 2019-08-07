using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Branch the commit is sitting on.
	/// Not a branch in terms of git, but a separate line of code evolution
	/// on commit graph.
	/// Merge and commit which parent has multiple childs are points
	/// where new branches are starting and the old ones are finishing.
	/// </summary>
	public class Branch
	{
		public int Id { get; set; }
		/// <summary>
		/// Binary mask with little-endian order to encode parent branches.
		/// </summary>
		public string Mask { get; set; }
		/// <summary>
		/// Offset of the mask in bits.
		/// </summary>
		public int MaskOffset { get; set; }

		public List<Commit> Commits { get; set; } = new List<Commit>();

		public void CombineMask(string mask, int maskOffset)
		{
			(Mask, MaskOffset) = CombineMask(Mask, MaskOffset, mask, maskOffset);
		}
		public static (string mask, int maskOffset) CombineMask(
			string mask1, int mask1Offset, string mask2, int mask2Offset)
		{
			int resultOffset = Math.Min(mask1Offset, mask2Offset);
			int resultMaskLength = Math.Max(
				mask1.Length + mask1Offset - resultOffset,
				mask2.Length + mask2Offset - resultOffset);

			StringBuilder resultMask = new StringBuilder(resultMaskLength);
			for (int i = 0; i < resultMaskLength; i++)
			{
				int bit1pos = i + resultOffset - mask1Offset;
				int bit2pos = i + resultOffset - mask2Offset;
				bool bit1 = bit1pos < 0 ? true : (bit1pos > mask1.Length-1 ? false : (mask1[bit1pos] == '1'));
				bool bit2 = bit2pos < 0 ? true : (bit2pos > mask2.Length-1 ? false : (mask2[bit2pos] == '1'));

				resultMask.Append(bit1 | bit2 ? '1' : '0');
			}
			int ones = 0;
			while (ones < resultMask.Length && resultMask[ones] == '1')
			{
				ones++;
			}
			if (ones > 1)
			{
				resultMask.Remove(0, ones - 1);
				resultOffset += ones - 1;
			}
			
			return (resultMask.ToString(), resultOffset);
		}
	}
}
