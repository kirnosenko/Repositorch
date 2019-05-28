using System;
using System.Linq;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class CodeFileMappingExtension
	{
		public static CodeFileMappingExpression AddFile(this ICommitMappingExpression exp, string path)
		{
			return new CodeFileMappingExpression(exp, path);
		}
		public static CodeFileMappingExpression File(this ICommitMappingExpression exp, string path)
		{
			return new CodeFileMappingExpression(
				exp,
				exp.Get<CodeFile>().Single(x =>
					x.Path == path && x.DeletedInCommitId == null
				)
			);
		}
	}

	public interface ICodeFileMappingExpression : ICommitMappingExpression
	{}

	public class CodeFileMappingExpression : EntityMappingExpression<CodeFile>, ICodeFileMappingExpression
	{
		public CodeFileMappingExpression(IRepositoryMappingExpression parentExp, string filePath)
			: base(parentExp)
		{
			entity = new CodeFile()
			{
				Path = filePath,
				AddedInCommit = CurrentEntity<Commit>()
			};
			Add(entity);
		}
		public CodeFileMappingExpression(IRepositoryMappingExpression parentExp, CodeFile file)
			: base(parentExp)
		{
			entity = file;
		}
		public ICodeFileMappingExpression Delete()
		{
			entity.DeletedInCommit = CurrentEntity<Commit>();
			return this;
		}
		public ICodeFileMappingExpression CopiedFrom(string sourseFilePath, string sourceRevision)
		{
			entity.SourceCommit = Get<Commit>()
				.Single(x => x.Revision == sourceRevision);
			entity.SourceFile = this.SelectionDSL()
				.Files().PathIs(sourseFilePath).ExistInRevision(sourceRevision).Single();
			return this;
		}
	}
}
