using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class CodeFileSelectionExtension
	{
		public static CodeFileSelectionExpression Files(this IRepositorySelectionExpression parentExp)
		{
			return new CodeFileSelectionExpression(parentExp);
		}
		public static CodeFileSelectionExpression DefectiveFiles(
			this CodeBlockSelectionExpression parentExp,
			string detectAfterRevision,
			string detectTillRevision)
		{
			return parentExp.ModifiedBy()
				.Modifications()
					.ContainCodeBlocks()
				.Commits()
					.AfterRevision(detectAfterRevision)
					.TillRevision(detectTillRevision)
					.AreBugFixes()
					.ContainModifications()
				.Files()
					.TouchedInCommits()
				.Reselect(x => x.Distinct());
		}
	}

	public class CodeFileSelectionExpression : EntitySelectionExpression<CodeFile,CodeFileSelectionExpression>
	{
		public CodeFileSelectionExpression(IRepositorySelectionExpression parentExp)
			: base(parentExp)
		{
		}
		public CodeFileSelectionExpression AddedInCommits()
		{
			return Reselect(s =>
				(from c in Selection<Commit>()
				join m in Queryable<Modification>() on c.Id equals m.CommitId
				join f in s on m.FileId equals f.Id
				where m.Action == Modification.FileAction.ADDED
				select f).Distinct()
			);
		}
		public CodeFileSelectionExpression RemovedInCommits()
		{
			return Reselect(s =>
				(from c in Selection<Commit>()
				join m in Queryable<Modification>() on c.Id equals m.CommitId
				join f in s on m.FileId equals f.Id
				where m.Action == Modification.FileAction.REMOVED
				select f).Distinct()
			);
		}
		public CodeFileSelectionExpression IdIs(int id)
		{
			return Reselect(s =>
				s.Where(x => x.Id == id)
			);
		}
		public CodeFileSelectionExpression PathIs(string filePath)
		{
			return Reselect(s =>
				s.Where(x => x.Path == filePath)
			);
		}
		public CodeFileSelectionExpression InDirectory(string dirPath)
		{
			if (! dirPath.EndsWith("/"))
			{
				dirPath += "/";
			}
			return PathStartsWith(dirPath);
		}
		public CodeFileSelectionExpression PathStartsWith(string pathBeginning)
		{
			return Reselect(s =>
				s.Where(x => x.Path.StartsWith(pathBeginning))
			);
		}
		public CodeFileSelectionExpression PathEndsWith(string pathEnding)
		{
			return Reselect(s =>
				s.Where(x => x.Path.EndsWith(pathEnding))
			);
		}
		public CodeFileSelectionExpression Exist()
		{
			return Reselect(s =>
				from f in s
				join m in Queryable<Modification>() on f.Id equals m.FileId
				group m by f into fileModifications
				where fileModifications.OrderByDescending(x => x.Id).First().Action != Modification.FileAction.REMOVED
				select fileModifications.Key
			);
		}
		public CodeFileSelectionExpression ExistInRevision(string revision)
		{
			var allCommitsTillRevision = this.Commits().TillRevision(revision);

			return Reselect(s =>
				from c in allCommitsTillRevision
				join m in Queryable<Modification>() on c.Id equals m.CommitId
				join f in s on m.FileId equals f.Id
				group m by f into fileModifications
				where fileModifications.OrderByDescending(x => x.Id).First().Action != Modification.FileAction.REMOVED
				select fileModifications.Key
			);
		}
		protected override CodeFileSelectionExpression Recreate()
		{
			return new CodeFileSelectionExpression(this);
		}
	}
}
