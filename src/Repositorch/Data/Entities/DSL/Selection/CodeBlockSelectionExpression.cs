using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class CodeBlockSelectionExtensions
	{
		public static CodeBlockSelectionExpression CodeBlocks(this IRepositorySelectionExpression parentExp)
		{
			return new CodeBlockSelectionExpression(parentExp);
		}
		public static ModificationSelectionExpression ContainCodeBlocks(this ModificationSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				(
					from m in s
					join cb in parentExp.Selection<CodeBlock>() on m.Id equals cb.ModificationId
					select m
				).Distinct()
			);
		}
		public static CommitSelectionExpression AreRefactorings(this CommitSelectionExpression parentExp)
		{
			return parentExp.AreNotBugFixes().Reselect(s =>
				(
					from c in s
					join m in parentExp.Queryable<Modification>() on c.Id equals m.CommitId
					join cb in parentExp.Queryable<CodeBlock>() on m.Id equals cb.ModificationId
					group cb by c into g
					select new
					{
						Commit = g.Key,
						Added = g.Where(x => x.Size > 0).Sum(x => x.Size),
						Removed = -g.Where(x => x.Size < 0).Sum(x => x.Size)
					}
				).Where(x => x.Removed / x.Added >= 1d / 2).Select(x => x.Commit)
			);
		}
	}

	public class CodeBlockSelectionExpression : EntitySelectionExpression<CodeBlock,CodeBlockSelectionExpression>
	{
		public CodeBlockSelectionExpression(IRepositorySelectionExpression parentExp)
			: base(parentExp)
		{
		}
		public CodeBlockSelectionExpression InModifications()
		{
			return Reselect(s =>
				from cb in s
				join m in Selection<Modification>() on cb.ModificationId equals m.Id
				select cb
			);
		}
		public CodeBlockSelectionExpression AddedInitiallyInCommits()
		{
			return Reselect(s =>
				from cb in s
				join c in Selection<Commit>() on cb.AddedInitiallyInCommitId equals c.Id
				select cb
			);
		}
		public CodeBlockSelectionExpression Added()
		{
			return Reselect(s => s.Where(x => x.Size > 0));
		}
		public CodeBlockSelectionExpression Deleted()
		{
			return Reselect(s => s.Where(x => x.Size < 0));
		}
		public CodeBlockSelectionExpression Modify()
		{
			return Reselect(s =>
				from cb in s
				join tcb in Queryable<CodeBlock>() on cb.TargetCodeBlockId equals tcb.Id
				select tcb
			);
		}
		public CodeBlockSelectionExpression ModifiedBy()
		{
			return Reselect(s =>
				from tcb in s
				join cb in Queryable<CodeBlock>() on tcb.Id equals cb.TargetCodeBlockId
				select cb
			);
		}
		public CodeBlockSelectionExpression InBugFixes()
		{
			return Reselect(s =>
				from cb in s
				join m in Queryable<Modification>() on cb.ModificationId equals m.Id
				join c in Queryable<Commit>() on m.CommitId equals c.Id
				join bf in Selection<BugFix>() on c.Id equals bf.CommitId
				select cb
			);
		}
		protected override CodeBlockSelectionExpression Recreate()
		{
			return new CodeBlockSelectionExpression(this);
		}
	}
}
