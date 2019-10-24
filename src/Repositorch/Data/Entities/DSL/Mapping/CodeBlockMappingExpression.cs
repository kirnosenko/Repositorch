using System;
using System.Linq;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class CodeBlockMappingExtension
	{
		public static CodeBlockMappingExpression Code(this IModificationMappingExpression exp, double size)
		{
			return new CodeBlockMappingExpression(exp, size);
		}
		public static CodeBlockMappingExpression CopyCode(this IModificationMappingExpression exp)
		{
			var modification = exp.CurrentEntity<Modification>();
			var sourceCommit = (
				from c in exp.GetReadOnly<Commit>().Where(x => x.Id == modification.SourceCommit.Id)
				join b in exp.GetReadOnly<Branch>() on c.BranchId equals b.Id
				select new { c.OrderedNumber, b.Mask }).Single();
			var commitsOnBranch = exp.SelectionDSL().Commits()
				.OnBranchBack(sourceCommit.Mask)
				.Where(x => x.OrderedNumber <= sourceCommit.OrderedNumber);

			return DoWithRemainingCode(
				exp, commitsOnBranch, modification.SourceFile.Id, CopyCodeBlock);
		}
		public static CodeBlockMappingExpression UpdateCode(
			this IModificationMappingExpression exp,
			Func<IModificationMappingExpression, string, double, CodeBlockMappingExpression> codeToExp)
		{
			var file = exp.CurrentEntity<CodeFile>();
			var currentCommitBranch = exp.CurrentEntity<Branch>();
			var commitsOnBranch = exp.SelectionDSL().Commits()
				.OnBranchBack(currentCommitBranch.Mask);

			return DoWithRemainingCode(
				exp, commitsOnBranch, file.Id, codeToExp);
		}
		public static CodeBlockMappingExpression RemoveCode(this IModificationMappingExpression exp)
		{
			var file = exp.CurrentEntity<CodeFile>();
			var currentCommitBranch = exp.CurrentEntity<Branch>();
			var commitsOnBranch = exp.SelectionDSL().Commits()
				.OnBranchBack(currentCommitBranch.Mask);

			return DoWithRemainingCode(
				exp, commitsOnBranch, file.Id, RemoveCodeBlock);
		}

		private static CodeBlockMappingExpression DoWithRemainingCode(
			this IModificationMappingExpression expression,
			IQueryable<Commit> commits,
			int fileId,
			Func<IModificationMappingExpression,string,double,CodeBlockMappingExpression> codeToExp)
		{
			var remaining—odeByRevision = (
				from m in expression.Get<Modification>().Where(x => x.FileId == fileId)
				join c in commits on m.CommitId equals c.Id
				join cb in expression.Get<CodeBlock>() on m.Id equals cb.ModificationId
				join tcb in expression.Get<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
				join tcbc in expression.Get<Commit>() on tcb.AddedInitiallyInCommitId equals tcbc.Id
				group cb.Size by tcbc.Revision into g
				select new
				{
					Revision = g.Key,
					Size = g.Sum()
				}).ToArray();

			CodeBlockMappingExpression codeExp = null;
			foreach (var codeForRevision in remaining—odeByRevision)
			{
				codeExp = codeToExp(
					codeExp ?? expression,
					codeForRevision.Revision,
					codeForRevision.Size) ?? codeExp;
			}

			return codeExp;
		}
		private static CodeBlockMappingExpression CopyCodeBlock(
			IModificationMappingExpression expression,
			string revision,
			double codeSize)
		{
			if (codeSize != 0)
			{
				var newExp = expression.Code(codeSize);
				newExp.CopiedFrom(revision);
				return newExp;
			}

			return null;
		}
		private static CodeBlockMappingExpression RemoveCodeBlock(
			IModificationMappingExpression expression,
			string revision,
			double codeSize)
		{
			if (codeSize != 0)
			{
				var newExp = expression.Code(-codeSize);
				newExp.ForCodeAddedInitiallyInRevision(revision);
				return newExp;
			}

			return null;
		}
	}

	public interface ICodeBlockMappingExpression : IModificationMappingExpression
	{}

	public class CodeBlockMappingExpression : EntityMappingExpression<CodeBlock>, ICodeBlockMappingExpression
	{
		public CodeBlockMappingExpression(IRepositoryMappingExpression parentExp, double size)
			: base(parentExp)
		{
			entity = new CodeBlock()
			{
				Size = size,
				Modification = CurrentEntity<Modification>(),
			};
			if (size > 0)
			{
				entity.AddedInitiallyInCommit = CurrentEntity<Commit>();
			}
			Add(entity);
		}
		public ICodeBlockMappingExpression CopiedFrom(string revision)
		{
			entity.AddedInitiallyInCommit = Get<Commit>()
				.Single(c => c.Revision == revision);
			return this;
		}
		public ICodeBlockMappingExpression ForCodeAddedInitiallyInRevision(string revision)
		{
			var file = CurrentEntity<Modification>().File;
			entity.AddedInitiallyInCommit = null;
			entity.TargetCodeBlock = this.SelectionDSL(true)
				.Commits().RevisionIs(revision)
				.Files().IdIs(file.Id)
				.Modifications().InFiles()
				.CodeBlocks().InModifications().AddedInitiallyInCommits()
				.OrderByDescending(x => x.Id).First();

			return this;
		}
	}
}
