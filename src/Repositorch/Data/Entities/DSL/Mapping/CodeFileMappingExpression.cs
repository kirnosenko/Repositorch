using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class CodeFileMappingExtension
	{
		public static CodeFileMappingExpression File(this ICommitMappingExpression exp, string filePath)
		{
			return new CodeFileMappingExpression(exp, filePath);
		}
	}

	public interface ICodeFileMappingExpression : ICommitMappingExpression
	{}

	public class CodeFileMappingExpression : EntityMappingExpression<CodeFile>, ICodeFileMappingExpression
	{
		public CodeFileMappingExpression(IRepositoryMappingExpression parentExp, string filePath)
			: base(parentExp)
		{
			entity = parentExp.Get<CodeFile>()
				.Where(f => f.Path == filePath).SingleOrDefault();
			if (entity == null)
			{
				entity = new CodeFile()
				{
					Path = filePath
				};
				Add(entity);
			}
		}
	}
}
