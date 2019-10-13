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
				where m.Action == TouchedFileAction.ADDED
				select f).Distinct()
			);
		}
		public CodeFileSelectionExpression RemovedInCommits()
		{
			return Reselect(s =>
				(from c in Selection<Commit>()
				join m in Queryable<Modification>() on c.Id equals m.CommitId
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
		/// Get files were added before the last mapped revision 
		/// and still exist in it.
		/// </summary>
		public CodeFileSelectionExpression Exist()
		{
			return TouchedInAndStillExistAfterCommits(null);
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
		private CodeFileSelectionExpression TouchedInAndStillExistAfterCommits(IQueryable<Commit> commits)
		{
			// This code is the result of exorcism to evict EF 'must be reducible node' error.
			// Reproduced on real DB not in-memory.

			IQueryable<IGrouping<CodeFile,Modification>> modificationsByFile = commits == null ?
				from f in Queryable<CodeFile>()
				join m in Queryable<Modification>() on f.Id equals m.FileId
				group m by f
				:
				from c in commits
				join m in Queryable<Modification>() on c.Id equals m.CommitId
				join f in Queryable<CodeFile>() on m.FileId equals f.Id
				group m by f;
			var filesWithLastAction = modificationsByFile
				.Select(fileModifications => new
				{
					File = fileModifications.Key,
					LastAction = fileModifications.OrderByDescending(x => x.Id).First().Action
				});
				
			return Reselect(s =>
				(
					from f in s
					join fa in filesWithLastAction on f.Id equals fa.File.Id
					select fa
				).Where(x => x.LastAction != TouchedFileAction.REMOVED)
				.Select(x => x.File));
		}

		protected override CodeFileSelectionExpression Recreate()
		{
			return new CodeFileSelectionExpression(this);
		}
	}
}
