using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class BugFixSelectionExtensions
	{
		public static BugFixSelectionExpression BugFixes(this IRepositorySelectionExpression parentExp)
		{
			return new BugFixSelectionExpression(parentExp);
		}
		public static CommitSelectionExpression AreBugFixes(this CommitSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				from c in s
				join bf in parentExp.Queryable<BugFix>() on c.Number equals bf.CommitNumber
				select c
			);
		}
		public static CommitSelectionExpression AreNotBugFixes(this CommitSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				from c in s
				join bf in parentExp.Queryable<BugFix>() on c.Number equals bf.CommitNumber into j
				from x in j.DefaultIfEmpty()
				where
					x == null
				select c
			);
		}
	}

	public class BugFixSelectionExpression : EntitySelectionExpression<BugFix, BugFixSelectionExpression>
	{
		public BugFixSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<BugFix> selection = null)
			: base(parentExp, selection)
		{
		}
		public BugFixSelectionExpression InCommits()
		{
			return Reselect((s) =>
				from bf in s
				join c in Selection<Commit>() on bf.CommitNumber equals c.Number
				select bf
			);
		}

		protected override BugFixSelectionExpression Recreate(IQueryable<BugFix> selection)
		{
			return new BugFixSelectionExpression(this, selection);
		}
	}
}
