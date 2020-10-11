using System;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class CommitAttributeMappingExtension
	{
		public static CommitAttributeMappingExpression IsMerge(
			this ICommitMappingExpression exp)
		{
			return new CommitAttributeMappingExpression(
				exp, CommitAttribute.MERGE, null);
		}
		public static CommitAttributeMappingExpression IsSplit(
			this ICommitMappingExpression exp)
		{
			return new CommitAttributeMappingExpression(
				exp, CommitAttribute.SPLIT, null);
		}
	}

	public interface ICommitAttributeMappingExpression : ICommitMappingExpression
	{}

	public class CommitAttributeMappingExpression : EntityMappingExpression<CommitAttribute>, ICommitAttributeMappingExpression
	{
		public CommitAttributeMappingExpression(
			IRepositoryMappingExpression parentExp, string type, string data)
			: base(parentExp)
		{
			entity = new CommitAttribute()
			{
				Type = type,
				Data = data,
				Commit = CurrentEntity<Commit>()
			};
			Add(entity);
		}
	}
}
