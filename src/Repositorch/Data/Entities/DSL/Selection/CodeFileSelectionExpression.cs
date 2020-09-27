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
		public CodeFileSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<CodeFile> selection = null)
			: base(parentExp, selection)
		{
		}
		public CodeFileSelectionExpression AddedInCommits()
		{
			return Reselect(s =>
				(from c in Selection<Commit>()
				join m in Queryable<Modification>() on c.Number equals m.CommitNumber
				join f in s on m.FileId equals f.Id
				where m.Action == TouchedFileAction.ADDED
				select f).Distinct()
			);
		}
		public CodeFileSelectionExpression RemovedInCommits()
		{
			return Reselect(s =>
				(from c in Selection<Commit>()
				join m in Queryable<Modification>() on c.Number equals m.CommitNumber
				join f in s on m.FileId equals f.Id
				where m.Action == TouchedFileAction.REMOVED
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
		public CodeFileSelectionExpression PathContains(string path)
		{
			return Reselect(s =>
				s.Where(x => x.Path.Contains(path))
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
		/// <summary>
		/// Get files were added before the revision and still exist in it.
		/// </summary>
		public CodeFileSelectionExpression ExistInRevision(string revision)
		{
			var allCommitsTillRevision = this.Commits().TillRevision(revision);

			return TouchedInAndStillExistAfterCommits(allCommitsTillRevision);
		}
		/// <summary>
		/// Get files were touched in selected commits and still exist 
		/// in the last revision from selected commits.
		/// </summary>
		public CodeFileSelectionExpression TouchedInAndStillExistAfterCommits()
		{
			return TouchedInAndStillExistAfterCommits(Selection<Commit>());
		}

		protected override CodeFileSelectionExpression Recreate(IQueryable<CodeFile> selection)
		{
			return new CodeFileSelectionExpression(this, selection);
		}
		private CodeFileSelectionExpression TouchedInAndStillExistAfterCommits(IQueryable<Commit> commits)
		{
			var fileLastMod =
				(from c in commits
				join m in Queryable<Modification>() on c.Number equals m.CommitNumber
				group m by m.FileId into fileModifications
				select new
				{
					FileId = fileModifications.Key,
					CommitNumber = fileModifications.Max(x => x.CommitNumber),
				});

			return Reselect(s =>
				from f in s
				join a in fileLastMod on f.Id equals a.FileId
				join m in Queryable<Modification>() on new { a.FileId, a.CommitNumber } equals new { m.FileId, m.CommitNumber }
				where m.Action != TouchedFileAction.REMOVED
				select f);
		}
	}
}
