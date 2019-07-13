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
			
			foreach (var codeByAddedCode in (
				from cb in exp.Get<CodeBlock>()
				join m in exp.Get<Modification>() on cb.ModificationId equals m.Id
				join f in exp.Get<CodeFile>() on m.FileId equals f.Id
				join c in exp.Get<Commit>() on m.CommitId equals c.Id
					let addedCodeID = cb.Size < 0 ? cb.TargetCodeBlockId : cb.Id
				where
					f.Id == exp.CurrentEntity<Modification>().SourceFile.Id &&
					c.OrderedNumber <= exp.CurrentEntity<Modification>().SourceCommit.OrderedNumber
				group cb.Size by addedCodeID
				)
			)
			{
				double currentCodeSize = codeByAddedCode.Sum();
				
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
						RevisionCodeBlockWasInitiallyAddedIn(exp, codeByAddedCode.Key ?? 0)
					);
				}
			}
				
			return lastCodeBlockExp;
		}
		public static CodeBlockMappingExpression DeleteCode(this IModificationMappingExpression exp)
		{
			CodeBlockMappingExpression lastCodeBlockExp = null;

			foreach (var codeByAddedCode in (
				from cb in exp.Get<CodeBlock>()
				join m in exp.Get<Modification>() on cb.ModificationId equals m.Id
				join f in exp.Get<CodeFile>() on m.FileId equals f.Id
					let addedCodeID = cb.Size < 0 ? cb.TargetCodeBlockId : cb.Id
				where
					f.Id == exp.CurrentEntity<CodeFile>().Id
				group cb.Size by addedCodeID
				)
			)
			{
				double currentCodeSize = codeByAddedCode.Sum();
				
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
						RevisionCodeBlockWasInitiallyAddedIn(exp, codeByAddedCode.Key ?? 0)
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
			entity.TargetCodeBlock = this.SelectionDSL()
					.Commits().RevisionIs(revision)
					.Files().IdIs(CurrentEntity<Modification>().File.Id)
					.Modifications().InFiles()
					.CodeBlocks().InModifications().AddedInitiallyInCommits().Single();

			return this;
		}
	}
}
