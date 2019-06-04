using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class BranchMappingExtension
	{
		public static BranchMappingExpression OnBranch(
			this ICommitMappingExpression exp, uint mask, uint maskOffset = 0)
		{
			return new BranchMappingExpression(exp, mask, maskOffset);
		}
	}

	public interface IBranchMappingExpression : ICommitMappingExpression
	{}

	public class BranchMappingExpression : EntityMappingExpression<Branch>, IBranchMappingExpression
	{
		public BranchMappingExpression(IRepositoryMappingExpression parentExp, uint mask, uint maskOffset)
			: base(parentExp)
		{
			entity = Get<Branch>().SingleOrDefault(b => b.Mask == mask && b.MaskOffset == maskOffset);
			if (entity == null)
			{
				entity = new Branch()
				{
					Mask = mask,
					MaskOffset = maskOffset,
				};
				Add(entity);
			}
			entity.Commits.Add(CurrentEntity<Commit>());
		}
	}
}
