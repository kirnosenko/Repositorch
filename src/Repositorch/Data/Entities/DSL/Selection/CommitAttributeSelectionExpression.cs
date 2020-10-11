using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class CommitAttributeSelectionExtensions
	{
		public static CommitSelectionExpression AreMerges(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.HasAttribute(CommitAttribute.MERGE);
		}
		public static CommitSelectionExpression AreNotMerges(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.HasNotAttribute(CommitAttribute.MERGE);
		}

		public static CommitSelectionExpression AreSplits(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.HasAttribute(CommitAttribute.SPLIT);
		}
		public static CommitSelectionExpression AreNotSplits(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.HasNotAttribute(CommitAttribute.SPLIT);
		}

		public static CommitSelectionExpression HasAttribute(
			this CommitSelectionExpression parentExp, string attributeType)
		{
			return parentExp.Reselect(s =>
				(from c in s
				join ca in parentExp.Queryable<CommitAttribute>()
					.Where(x => x.Type == attributeType) on c.Number equals ca.CommitNumber
				select c).Distinct()
			);
		}
		public static CommitSelectionExpression HasNotAttribute(
			this CommitSelectionExpression parentExp, string attributeType)
		{
			return parentExp.Reselect(s =>
				from c in s
				join ca in parentExp.Queryable<CommitAttribute>()
					.Where(x => x.Type == attributeType) on c.Number equals ca.CommitNumber into caGroup
				from caNullable in caGroup.DefaultIfEmpty()
				where caNullable == null
				select c
			);
		}
	}

	public class CommitAttributeSelectionExpression : EntitySelectionExpression<CommitAttribute, CommitAttributeSelectionExpression>
	{
		private CommitAttributeSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<CommitAttribute> selection = null)
			: base(parentExp, selection)
		{
		}
		
		protected override CommitAttributeSelectionExpression Recreate(IQueryable<CommitAttribute> selection)
		{
			return new CommitAttributeSelectionExpression(this, selection);
		}
	}
}
