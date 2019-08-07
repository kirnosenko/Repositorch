using System;
using System.Linq;
using System.Linq.Expressions;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class BranchSelectionExtensions
	{
		public static BranchSelectionExpression Branches(this IRepositorySelectionExpression parentExp)
		{
			return new BranchSelectionExpression(parentExp);
		}
		public static CommitSelectionExpression OnBranchBack(
			this CommitSelectionExpression parentExp, string mask, int maskOffset = 0)
		{
			return parentExp.Reselect(s =>
				from c in s
				join br in parentExp.Queryable<Branch>() on c.BranchId equals br.Id
				let branch_bit_pos = br.Mask.Length + br.MaskOffset - 1 - maskOffset
				where
					(branch_bit_pos < 0)
					||
					(branch_bit_pos < mask.Length && mask[branch_bit_pos] == '1')
				select c);
		}
		public static CommitSelectionExpression OnBranchForward(
			this CommitSelectionExpression parentExp, string mask, int maskOffset = 0)
		{
			return parentExp.Reselect(s =>
				from c in s
				join br in parentExp.Queryable<Branch>() on c.BranchId equals br.Id
				let branch_bit_pos = mask.Length + maskOffset - 1 - br.MaskOffset
				where
					(branch_bit_pos < 0)
					||
					(branch_bit_pos < br.Mask.Length && br.Mask[branch_bit_pos] == '1')
				select c);
		}
		public static CommitSelectionExpression BeforeRevision(
			this CommitSelectionExpression parentExp, string revision)
		{
			return RelativeCommitSelection(
				parentExp,
				revision,
				(s, number) => s.BeforeNumber(number),
				(s, mask, offset) => s.OnBranchBack(mask, offset));
		}
		public static CommitSelectionExpression TillRevision(
			this CommitSelectionExpression parentExp, string revision)
		{
			return RelativeCommitSelection(
				parentExp,
				revision,
				(s, number) => s.TillNumber(number),
				(s, mask, offset) => s.OnBranchBack(mask, offset));
		}
		public static CommitSelectionExpression AfterRevision(
			this CommitSelectionExpression parentExp, string revision)
		{
			return RelativeCommitSelection(
				parentExp,
				revision,
				(s, number) => s.AfterNumber(number),
				(s, mask, offset) => s.OnBranchForward(mask, offset));
		}
		public static CommitSelectionExpression FromRevision(
			this CommitSelectionExpression parentExp, string revision)
		{
			return RelativeCommitSelection(
				parentExp,
				revision,
				(s, number) => s.FromNumber(number),
				(s, mask, offset) => s.OnBranchForward(mask, offset));
		}

		private static CommitSelectionExpression RelativeCommitSelection(
			this CommitSelectionExpression parentExp,
			string revision,
			Func<CommitSelectionExpression,int,CommitSelectionExpression> numberFilter,
			Func<CommitSelectionExpression,string,int,CommitSelectionExpression> branchFilter)
		{
			if (revision == null)
			{
				return parentExp;
			}
			var revisionData = (
				from c in parentExp.Queryable<Commit>().Where(x => x.Revision == revision)
				join b in parentExp.Queryable<Branch>() on c.BranchId equals b.Id
				select new
				{
					Number = c.OrderedNumber,
					Mask = b.Mask,
					MaskOffset = b.MaskOffset
				}).Single();
			return parentExp
				.Reselect(s => numberFilter(s, revisionData.Number))
				.Reselect(s => branchFilter(s, revisionData.Mask, revisionData.MaskOffset));
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
