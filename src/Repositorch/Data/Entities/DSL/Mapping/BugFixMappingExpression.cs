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

	public class BugFixMappingExpression : EntityMappingExpression<CommitAttribute>, IBugFixMappingExpression
	{
		public BugFixMappingExpression(IRepositoryMappingExpression parentExp)
			: base(parentExp)
		{
			entity = new CommitAttribute()
			{
				Type = CommitAttribute.FIX,
				Commit = CurrentEntity<Commit>()
			};
			Add(entity);
		}
	}
}
