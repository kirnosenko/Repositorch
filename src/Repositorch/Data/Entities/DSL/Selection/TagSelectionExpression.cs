using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class TagSelectionExtensions
	{
		public static TagSelectionExpression Tags(this IRepositorySelectionExpression parentExp)
		{
			return new TagSelectionExpression(parentExp);
		}
		public static CommitSelectionExpression WithTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			return parentExp.Reselect(s =>
				from c in s
				join t in parentExp.Selection<Tag>() on c.Number equals t.CommitNumber
				where t.Title == tag
				select c
			);
		}
		public static CommitSelectionExpression WithTags(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				(from c in s
				join t in parentExp.Selection<Tag>() on c.Number equals t.CommitNumber
				select c).Distinct()
			);
		}
		public static CommitSelectionExpression BeforeTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			var commitWithTag = parentExp.Fixed().WithTag(tag).SingleOrDefault();

			return parentExp.BeforeRevision(commitWithTag?.Revision);
		}
		public static CommitSelectionExpression TillTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			var commitWithTag = parentExp.Fixed().WithTag(tag).SingleOrDefault();

			return parentExp.TillRevision(commitWithTag?.Revision);
		}
		public static CommitSelectionExpression AfterTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			var commitWithTag = parentExp.Fixed().WithTag(tag).SingleOrDefault();

			return parentExp.AfterRevision(commitWithTag?.Revision);
		}
		public static CommitSelectionExpression FromTag(
			this CommitSelectionExpression parentExp, string tag)
		{
			var commitWithTag = parentExp.Fixed().WithTag(tag).SingleOrDefault();

			return parentExp.FromRevision(commitWithTag?.Revision);
		}
	}

	public class TagSelectionExpression : EntitySelectionExpression<Tag, TagSelectionExpression>
	{
		public TagSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<Tag> selection = null)
			: base(parentExp, selection)
		{
		}
		public TagSelectionExpression OfCommits()
		{
			return Reselect((s) =>
				from t in s
				join c in Selection<Commit>() on t.CommitNumber equals c.Number
				select t
			);
		}
		public TagSelectionExpression WithTitles(params string[] titles)
		{
			return Reselect((s) =>
				from t in s
				where titles.Contains(t.Title)
				select t
			);
		}

		protected override TagSelectionExpression Recreate(IQueryable<Tag> selection)
		{
			return new TagSelectionExpression(this, selection);
		}
	}
}
