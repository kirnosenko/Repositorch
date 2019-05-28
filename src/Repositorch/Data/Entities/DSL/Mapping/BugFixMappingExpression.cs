using System;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class BugFixMappingExtension
	{
		public static BugFixMappingExpression IsBugFix(this ICommitMappingExpression exp)
		{
			return new BugFixMappingExpression(exp);
		}
	}

	public interface IBugFixMappingExpression : ICommitMappingExpression
	{}

	public class BugFixMappingExpression : EntityMappingExpression<BugFix>, IBugFixMappingExpression
	{
		public BugFixMappingExpression(IRepositoryMappingExpression parentExp)
			: base(parentExp)
		{
			entity = new BugFix()
			{
				Commit = CurrentEntity<Commit>()
			};
			Add(entity);
		}
	}
}
