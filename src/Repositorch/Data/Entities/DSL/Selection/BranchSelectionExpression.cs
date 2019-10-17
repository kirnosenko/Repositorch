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
			this CommitSelectionExpression parentExp, BranchMask mask)
		{
			return parentExp.Reselect(s =>
				from c in s
				join br in parentExp.Queryable<Branch>() on c.BranchId equals br.Id
				let branch_bit_pos = br.Mask.Data.Length + br.Mask.Offset - 1 - mask.Offset
				where
					(branch_bit_pos < 0)
					||
					(branch_bit_pos < mask.Data.Length && mask.Data[branch_bit_pos] == '1')
				select c);
		}
		public static CommitSelectionExpression OnBranchForward(
			this CommitSelectionExpression parentExp, BranchMask mask)
		{
			return parentExp.Reselect(s =>
				from c in s
				join br in parentExp.Queryable<Branch>() on c.BranchId equals br.Id
				let branch_bit_pos = mask.Data.Length + mask.Offset - 1 - br.Mask.Offset
				where
					(branch_bit_pos < 0)
					||
					(branch_bit_pos < br.Mask.Data.Length && br.Mask.Data[branch_bit_pos] == '1')
				select c);
		}
		public static CommitSelectionExpression BeforeRevision(
			this CommitSelectionExpression parentExp, string revision)
		{
			return RelativeCommitSelection(
				parentExp,
				revision,
				(s, number) => s.BeforeNumber(number),
				(s, mask) => s.OnBranchBack(mask));
		}
		public static CommitSelectionExpression TillRevision(
			this CommitSelectionExpression parentExp, string revision)
		{
			return RelativeCommitSelection(
				parentExp,
				revision,
				(s, number) => s.TillNumber(number),
				(s, mask) => s.OnBranchBack(mask));
		}
		public static CommitSelectionExpression AfterRevision(
			this CommitSelectionExpression parentExp, string revision)
		{
			return RelativeCommitSelection(
				parentExp,
				revision,
				(s, number) => s.AfterNumber(number),
				(s, mask) => s.OnBranchForward(mask));
		}
		public static CommitSelectionExpression FromRevision(
			this CommitSelectionExpression parentExp, string revision)
		{
			return RelativeCommitSelection(
				parentExp,
				revision,
				(s, number) => s.FromNumber(number),
				(s, mask) => s.OnBranchForward(mask));
		}

		private static CommitSelectionExpression RelativeCommitSelection(
			this CommitSelectionExpression parentExp,
			string revision,
			Func<CommitSelectionExpression,int,CommitSelectionExpression> numberFilter,
			Func<CommitSelectionExpression,BranchMask,CommitSelectionExpression> branchFilter)
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
					Branch = b,
				}).Single();
			return parentExp
				.Reselect(s => numberFilter(s, revisionData.Number))
				.Reselect(s => branchFilter(s, revisionData.Branch.Mask));
		}
	}

	public class BranchSelectionExpression : EntitySelectionExpression<Branch, BranchSelectionExpression>
	{
		public BranchSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<Branch> selection = null)
			: base(parentExp, selection)
		{
		}
		public BranchSelectionExpression OfCommits()
		{
			return Reselect((s) =>
				(from c in Selection<Commit>()
				 join b in s on c.BranchId equals b.Id
				 select b).Distinct()
			);
		}

		protected override BranchSelectionExpression Recreate(IQueryable<Branch> selection)
		{
			return new BranchSelectionExpression(this, selection);
		}
	}
}
