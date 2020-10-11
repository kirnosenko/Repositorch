using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class TagSelectionExtensions
	{
		public static CommitSelectionExpression AreTagged(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.HasAttribute(CommitAttribute.TAG);
		}
		public static CommitSelectionExpression AreNotTagged(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.HasNotAttribute(CommitAttribute.TAG);
		}
		public static CommitSelectionExpression WithTags(
			this CommitSelectionExpression parentExp, params string[] tags)
		{
			return parentExp.Reselect(s =>
				(from c in s
				 join ca in parentExp.Selection<CommitAttribute>() on c.Number equals ca.CommitNumber
				 where
					 ca.Type == CommitAttribute.TAG &&
					 tags.Contains(ca.Data)
				 select c).Distinct()
			);
		}
		public static CommitSelectionExpression BeforeTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			var commitWithTag = parentExp.Fixed().WithTags(tag).SingleOrDefault();
			if (commitWithTag == null)
			{
				return parentExp.Reselect(c => c.Take(0));
			}

			return parentExp.BeforeRevision(commitWithTag.Revision);
		}
		public static CommitSelectionExpression TillTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			var commitWithTag = parentExp.Fixed().WithTags(tag).SingleOrDefault();
			if (commitWithTag == null)
			{
				return parentExp.Reselect(c => c.Take(0));
			}

			return parentExp.TillRevision(commitWithTag.Revision);
		}
		public static CommitSelectionExpression AfterTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			var commitWithTag = parentExp.Fixed().WithTags(tag).SingleOrDefault();
			if (commitWithTag == null)
			{
				return parentExp.Reselect(c => c.Take(0));
			}

			return parentExp.AfterRevision(commitWithTag.Revision);
		}
		public static CommitSelectionExpression FromTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			var commitWithTag = parentExp.Fixed().WithTags(tag).SingleOrDefault();
			if (commitWithTag == null)
			{
				return parentExp.Reselect(c => c.Take(0));
			}

			return parentExp.FromRevision(commitWithTag.Revision);
		}
	}

	public class TagSelectionExpression : EntitySelectionExpression<CommitAttribute, TagSelectionExpression>
	{
		private TagSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<CommitAttribute> selection = null)
			: base(parentExp, selection)
		{
		}
		
		protected override TagSelectionExpression Recreate(IQueryable<CommitAttribute> selection)
		{
			return new TagSelectionExpression(this, selection);
		}
	}
}
