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
		/*
		public static ProjectFileSelectionExpression DefectiveFiles(
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
		}*/
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
				from f in s
				join c in Selection<Commit>() on f.AddedInCommitId equals c.Id
				select f
			);
		}
		public CodeFileSelectionExpression Deleted()
		{
			return Reselect(s =>
				from f in s
				where
					f.DeletedInCommitId != null
				select f
			);
		}
		public CodeFileSelectionExpression DeletedInCommits()
		{
			return Reselect(s =>
				from f in s
				join c in Selection<Commit>() on f.DeletedInCommitId equals c.Id
				select f
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
				where f.DeletedInCommitId == null
				select f
			);
		}
		public CodeFileSelectionExpression ExistInRevision(string revision)
		{
			var revisionNumber = Queryable<Commit>().Single(x => x.Revision == revision).OrderedNumber;
			return Reselect(s =>
				from f in s
				join ac in Queryable<Commit>() on f.AddedInCommitId equals ac.Id
				join dc in Queryable<Commit>() on f.DeletedInCommitId equals dc.Id into deletedInCommit
				from dcn in deletedInCommit.DefaultIfEmpty()
				where ac.OrderedNumber <= revisionNumber && (dcn == null || dcn.OrderedNumber > revisionNumber)
				select f
			);
		}
		protected override CodeFileSelectionExpression Recreate()
		{
			return new CodeFileSelectionExpression(this);
		}
	}
}
