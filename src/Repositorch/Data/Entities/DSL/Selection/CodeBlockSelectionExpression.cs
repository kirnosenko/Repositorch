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
			return parentExp.Reselect(s => s.Where(commit =>
				(from c in s
				 join m in parentExp.Queryable<Modification>() on c.Number equals m.CommitNumber
				 join cb in parentExp.Queryable<CodeBlock>() on m.Id equals cb.ModificationId
				 where cb.TargetCodeBlock == null || cb.Size < 0
				 group cb.Size by c.Number)
				.Where(x => x.Sum() <= 0).Select(x => x.Key).Contains(commit.Number)));
		}
	}

	public class CodeBlockSelectionExpression : EntitySelectionExpression<CodeBlock,CodeBlockSelectionExpression>
	{
		public CodeBlockSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<CodeBlock> selection = null)
			: base(parentExp, selection)
		{
		}
		public CodeBlockSelectionExpression InCommits()
		{
			return Reselect(s =>
				from cb in s
				join m in Queryable<Modification>() on cb.ModificationId equals m.Id
				join c in Selection<Commit>() on m.CommitNumber equals c.Number
				select cb
			);
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
				join c in Selection<Commit>() on cb.AddedInitiallyInCommitNumber equals c.Number
				select cb
			);
		}
		public CodeBlockSelectionExpression Added()
		{
			return Reselect(s => s.Where(x => x.TargetCodeBlockId == null));
		}
		public CodeBlockSelectionExpression Removed()
		{
			return Reselect(s => s.Where(x => x.Size < 0));
		}
		public CodeBlockSelectionExpression Targeted()
		{
			return Reselect(s => s.Where(x => x.TargetCodeBlock != null ));
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

		protected override CodeBlockSelectionExpression Recreate(IQueryable<CodeBlock> selection)
		{
			return new CodeBlockSelectionExpression(this, selection);
		}
	}
}
