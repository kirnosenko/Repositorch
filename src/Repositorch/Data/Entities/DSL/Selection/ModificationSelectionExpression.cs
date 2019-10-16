using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class ModificationSelectionExtensions
	{
		public static ModificationSelectionExpression Modifications(this IRepositorySelectionExpression parentExp)
		{
			return new ModificationSelectionExpression(parentExp);
		}
		public static CommitSelectionExpression ContainModifications(this CommitSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				(
					from c in s
					join m in parentExp.Selection<Modification>() on c.Id equals m.CommitId
					select c
				).Distinct()
			);
		}
		public static CodeFileSelectionExpression ContainModifications(this CodeFileSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				(
					from f in s
					join m in parentExp.Selection<Modification>() on f.Id equals m.FileId
					select f
				).Distinct()
			);
		}
		public static CommitSelectionExpression TouchFiles(this CommitSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				(
					from c in s
					join m in parentExp.Queryable<Modification>() on c.Id equals m.CommitId
					join f in parentExp.Selection<CodeFile>() on m.FileId equals f.Id
					select c
				).Distinct()
			);
		}
		public static CodeFileSelectionExpression TouchedInCommits(this CodeFileSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				(
					from f in s
					join m in parentExp.Queryable<Modification>() on f.Id equals m.FileId
					join c in parentExp.Selection<Commit>() on m.CommitId equals c.Id
					select f
				).Distinct()
			);
		}
	}

	public class ModificationSelectionExpression : EntitySelectionExpression<Modification,ModificationSelectionExpression>
	{
		public ModificationSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<Modification> selection = null)
			: base(parentExp, selection)
		{
		}
		public ModificationSelectionExpression InCommits()
		{
			return Reselect((s) =>
				from m in s
				join c in Selection<Commit>() on m.CommitId equals c.Id
				select m
			);
		}
		public ModificationSelectionExpression InFiles()
		{
			return Reselect((s) =>
				from m in s
				join f in Selection<CodeFile>() on m.FileId equals f.Id
				select m
			);
		}

		protected override ModificationSelectionExpression Recreate(IQueryable<Modification> selection)
		{
			return new ModificationSelectionExpression(this, selection);
		}
	}
}
