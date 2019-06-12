using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class BranchSelectionExtensions
	{
		public static BranchSelectionExpression Branches(this IRepositorySelectionExpression parentExp)
		{
			return new BranchSelectionExpression(parentExp);
		}
		public static CommitSelectionExpression OnBranchBack(
			this CommitSelectionExpression parentExp, uint mask, uint maskOffset = 0)
		{
			return parentExp.Reselect(s =>
				from c in s
				join br in parentExp.Queryable<Branch>() on c.BranchId equals br.Id
				let offset_delta = (int)(maskOffset - br.MaskOffset)
				let right_shifted_mask = offset_delta < 0 ? ((mask >> -offset_delta) | (Branch.MaskMax << (Branch.MaskSize + offset_delta))) : 0
				let left_shifted_mask = offset_delta > 0 ? ((mask << offset_delta) | (Branch.MaskMax >> (Branch.MaskSize - offset_delta))) : 0
				where
					offset_delta >= Branch.MaskSize
					||
					(
						offset_delta > -Branch.MaskSize
						&&
						(
							(offset_delta == 0 && (br.Mask | mask) == mask)
							||
							(offset_delta < 0 && ((br.Mask | right_shifted_mask) == right_shifted_mask))
							||
							(offset_delta > 0 && ((br.Mask | left_shifted_mask) == left_shifted_mask))
						)
					)
				select c);
		}
		public static CommitSelectionExpression OnBranchForward(
			this CommitSelectionExpression parentExp, uint mask, uint maskOffset = 0)
		{
			return parentExp.Reselect(s =>
				from c in s
				join br in parentExp.Queryable<Branch>() on c.BranchId equals br.Id
				let offset_delta = (int)(maskOffset - br.MaskOffset)
				let right_shifted_mask = offset_delta < 0 ? (mask >> -offset_delta) : 0
				let left_shifted_mask = offset_delta > 0 ? (mask << offset_delta) : 0
				where
					offset_delta <= -Branch.MaskSize
					||
					(
						offset_delta < Branch.MaskSize
						&&
						(
							(offset_delta == 0 && (br.Mask & mask) == mask)
							||
							(offset_delta < 0 && ((br.Mask & right_shifted_mask) == right_shifted_mask))
							||
							(offset_delta > 0 && ((br.Mask & left_shifted_mask) == left_shifted_mask))
						)
					)
				select c);
		}
	}

	public class BranchSelectionExpression : EntitySelectionExpression<Branch, BranchSelectionExpression>
	{
		public BranchSelectionExpression(IRepositorySelectionExpression parentExp)
			: base(parentExp)
		{
		}
		protected override BranchSelectionExpression Recreate()
		{
			return new BranchSelectionExpression(this);
		}
	}
}
