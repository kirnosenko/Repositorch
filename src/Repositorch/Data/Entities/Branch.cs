using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repositorch.Data.Entities
{	
	public class BranchMask
	{
		/// <summary>
		/// Binary mask with little-endian order.
		/// </summary>
		public string Data { get; set; }
		/// <summary>
		/// Offset of the mask in bits.
		/// </summary>
		public int Offset { get; set; }
		/// <summary>
		/// Total size of the mask in bits.
		/// </summary>
		public int Size
		{
			get
			{
				return Data.Length + Offset;
			}
		}

		public static implicit operator BranchMask(string mask)
		{
			return Create(mask);
		}
		public static implicit operator BranchMask((string data, int offset) mask)
		{
			return Create(mask.data, mask.offset);
		}
		public static BranchMask Create(string mask, int offset = 0)
		{
			return new BranchMask()
			{
				Data = mask,
				Offset = offset
			};
		}
		public static BranchMask And(params BranchMask[] masks)
		{
			return Logic((b1, b2) => (b1 == '1' && b2 == '1') ? '1' : '0', masks);
		}
		public static BranchMask Or(params BranchMask[] masks)
		{
			return Logic((b1, b2) => (b1 == '1' || b2 == '1') ? '1' : '0', masks);
		}
		public static BranchMask Xor(params BranchMask[] masks)
		{
			return Logic((b1, b2) => (b1 != b2) ? '1' : '0', masks);
		}
		private static BranchMask Logic(Func<char,char,char> op, params BranchMask[] masks)
		{
			masks = masks.OrderByDescending(m => m.Size).ToArray();
			StringBuilder result = new StringBuilder(Unshift(masks[0]));

			foreach (var m in masks.Skip(1))
			{
				for (int i = 0; i < result.Length; i++)
				{
					var maskBit = (i < m.Offset) ? '1' : (i >= m.Size) ? '0' : m.Data[i-m.Offset];
					result[i] = op(result[i], maskBit);
				}
			}

			return Shift(result.ToString());
		}
		private static string Unshift(BranchMask mask)
		{
			if (mask.Offset == 0)
			{
				return mask.Data;
			}
			return new string('1', mask.Offset) + mask.Data;
		}
		private static BranchMask Shift(string mask)
		{
			mask = mask.TrimEnd('0');
			
			int offset = 0;
			int ones = 0;
			while (ones < mask.Length && mask[ones] == '1')
			{
				ones++;
			}
			if (ones > 1)
			{
				ones--;
				mask = mask.Substring(ones, mask.Length-ones);
				offset += ones;
			}

			return new BranchMask()
			{
				Data = mask,
				Offset = offset
			};
		}
	}

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
		/// Branch mask to encode another branches were merged to this one.
		/// </summary>
		public BranchMask Mask { get; set; }
		
		/// <summary>
		/// Commits on the branch.
		/// </summary>
		public List<Commit> Commits { get; set; } = new List<Commit>();
	}
}
