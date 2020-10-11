using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class BugFixSelectionExtensions
	{
		public static CommitSelectionExpression AreBugFixes(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.HasAttribute(CommitAttribute.FIX);
		}
		public static CommitSelectionExpression AreNotBugFixes(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.HasNotAttribute(CommitAttribute.FIX);
		}
	}

	public class BugFixSelectionExpression : EntitySelectionExpression<CommitAttribute, BugFixSelectionExpression>
	{
		private BugFixSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<CommitAttribute> selection = null)
			: base(parentExp, selection)
		{
		}
		
		protected override BugFixSelectionExpression Recreate(IQueryable<CommitAttribute> selection)
		{
			return new BugFixSelectionExpression(this, selection);
		}
	}
}
