using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class ModificationMappingExtension
	{
		public static ModificationMappingExpression Modified(this ICodeFileMappingExpression exp)
		{
			return new ModificationMappingExpression(exp);
		}
	}

	public interface IModificationMappingExpression : ICodeFileMappingExpression
	{}

	public class ModificationMappingExpression : EntityMappingExpression<Modification>, IModificationMappingExpression
	{
		public ModificationMappingExpression(IRepositoryMappingExpression parentExp)
			: base(parentExp)
		{
			entity = new Modification()
			{
				Commit = CurrentEntity<Commit>(),
				File = CurrentEntity<CodeFile>()
			};
			AddEntity();
		}
	}
}
