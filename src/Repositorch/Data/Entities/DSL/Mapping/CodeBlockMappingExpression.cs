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
			CodeBlockMappingExpression lastCodeBlockExp = null;
			var modification = exp.CurrentEntity<Modification>();

			var codeByFirstBlock = (
				from cb in exp.GetReadOnly<CodeBlock>()
				join m in exp.GetReadOnly<Modification>() on cb.ModificationId equals m.Id
				join f in exp.Get<CodeFile>() on m.FileId equals f.Id
				join c in exp.Get<Commit>() on m.CommitId equals c.Id
				join tcb in exp.Get<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
				where f.Id == modification.SourceFile.Id
					&& c.OrderedNumber <= modification.SourceCommit.OrderedNumber
				group cb.Size by tcb.Id into g
				select new
				{
					Id = g.Key,
					Size = g.Sum()
				}).ToArray();

			foreach (var codeForFirstBlock in codeByFirstBlock)
			{
				double currentCodeSize = codeForFirstBlock.Size;
				
				if (currentCodeSize != 0)
				{
					if (lastCodeBlockExp == null)
					{
						lastCodeBlockExp = exp.Code(currentCodeSize);
					}
					else
					{
						lastCodeBlockExp = lastCodeBlockExp.Code(currentCodeSize);
					}
					lastCodeBlockExp.CopiedFrom(
						RevisionCodeBlockWasInitiallyAddedIn(exp, codeForFirstBlock.Id)
					);
				}
			}
				
			return lastCodeBlockExp;
		}
		public static CodeBlockMappingExpression DeleteCode(this IModificationMappingExpression exp)
		{
			CodeBlockMappingExpression lastCodeBlockExp = null;

			var codeByFistBlock = (
				from cb in exp.GetReadOnly<CodeBlock>()
				join m in exp.GetReadOnly<Modification>() on cb.ModificationId equals m.Id
				join f in exp.Get<CodeFile>() on m.FileId equals f.Id
				join tcb in exp.Get<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
				where f.Id == exp.CurrentEntity<CodeFile>().Id
				group cb.Size by tcb.Id into g
				select new
				{
					Id = g.Key,
					Size = g.Sum()
				}).ToArray();

			foreach (var codeForFirstBlock in codeByFistBlock)
			{
				double currentCodeSize = codeForFirstBlock.Size;
				
				if (currentCodeSize != 0)
				{
					if (lastCodeBlockExp == null)
					{
						lastCodeBlockExp = exp.Code(- currentCodeSize);
					}
					else
					{
						lastCodeBlockExp = lastCodeBlockExp.Code(- currentCodeSize);
					}
					lastCodeBlockExp.ForCodeAddedInitiallyInRevision(
						RevisionCodeBlockWasInitiallyAddedIn(exp, codeForFirstBlock.Id)
					);
				}
			}
			
			return lastCodeBlockExp;
		}
		private static string RevisionCodeBlockWasInitiallyAddedIn(IRepository repository, int codeBlockID)
		{
			return repository.Get<Commit>()
				.Single(c => c.Id == repository.Get<CodeBlock>()
					.Single(cb => cb.Id == codeBlockID).AddedInitiallyInCommitId
				).Revision;
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
