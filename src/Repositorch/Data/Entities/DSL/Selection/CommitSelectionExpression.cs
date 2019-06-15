using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class CommitSelectionExtensions
	{
		public static CommitSelectionExpression Commits(this IRepositorySelectionExpression parentExp)
		{
			return new CommitSelectionExpression(parentExp);
		}
	}

	public class CommitSelectionExpression : EntitySelectionExpression<Commit, CommitSelectionExpression>
	{
		public CommitSelectionExpression(IRepositorySelectionExpression parentExp)
			: base(parentExp)
		{
		}
		public CommitSelectionExpression RevisionIs(string revision)
		{
			return Reselect(s => s.Where(x => x.Revision == revision));
		}
		public CommitSelectionExpression RevisionIsNot(string revision)
		{
			return Reselect(s => s.Where(x => x.Revision != revision));
		}
		public CommitSelectionExpression BeforeDate(DateTime date)
		{
			return Reselect(s => s.Where(x => x.Date < date));
		}
		public CommitSelectionExpression TillDate(DateTime date)
		{
			return Reselect(s => s.Where(x => x.Date <= date));
		}
		public CommitSelectionExpression AfterDate(DateTime date)
		{
			return Reselect(s => s.Where(x => x.Date > date));
		}
		public CommitSelectionExpression FromDate(DateTime date)
		{
			return Reselect(s => s.Where(x => x.Date >= date));
		}
		public CommitSelectionExpression BeforeNumber(int orderedNumber)
		{
			return Reselect(s =>
				from c in s
				where
					c.OrderedNumber < orderedNumber
				select c
			);
		}
		public CommitSelectionExpression TillNumber(int orderedNumber)
		{
			return Reselect(s =>
				from c in s
				where
					c.OrderedNumber <= orderedNumber
				select c
			);
		}
		public CommitSelectionExpression FromNumber(int orderedNumber)
		{
			return Reselect(s =>
				from c in s
				where
					c.OrderedNumber >= orderedNumber
				select c
			);
		}
		public CommitSelectionExpression AfterNumber(int orderedNumber)
		{
			return Reselect(s =>
				from c in s
				where
					c.OrderedNumber > orderedNumber
				select c
			);
		}
		
		protected override CommitSelectionExpression Recreate()
		{
			return new CommitSelectionExpression(this);
		}
	}
}
