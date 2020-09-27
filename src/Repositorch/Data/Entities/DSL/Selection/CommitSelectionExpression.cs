using System;
using System.Collections.Generic;
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
		public CommitSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<Commit> selection = null)
			: base(parentExp, selection)
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
		public CommitSelectionExpression RevisionIsIn(IEnumerable<string> revisions)
		{
			return Reselect(s => s.Where(x => revisions.Contains(x.Revision)));
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
		public CommitSelectionExpression BeforeNumber(int number)
		{
			return Reselect(s =>
				from c in s
				where
					c.Number < number
				select c
			);
		}
		public CommitSelectionExpression TillNumber(int number)
		{
			return Reselect(s =>
				from c in s
				where
					c.Number <= number
				select c
			);
		}
		public CommitSelectionExpression FromNumber(int number)
		{
			return Reselect(s =>
				from c in s
				where
					c.Number >= number
				select c
			);
		}
		public CommitSelectionExpression AfterNumber(int number)
		{
			return Reselect(s =>
				from c in s
				where
					c.Number > number
				select c
			);
		}

		protected override CommitSelectionExpression Recreate(IQueryable<Commit> selection)
		{
			return new CommitSelectionExpression(this, selection);
		}
	}
}
