using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class BranchMappingExtension
	{
		/// <summary>
		/// Create a new branch from scratch without any parent branch.
		/// </summary>
		public static BranchMappingExpression OnFreshBranch(
			this ICommitMappingExpression exp)
		{
			return new BranchMappingExpression(exp);
		}
		/// <summary>
		/// Create a new branch or use the existent one with specified mask and offset.
		/// </summary>
		public static BranchMappingExpression OnBranch(
			this ICommitMappingExpression exp, string mask, int maskOffset = 0)
		{
			return new BranchMappingExpression(exp, mask, maskOffset, false);
		}
		/// <summary>
		/// Create a new sub-branch based on parent branch mask and offset.
		/// </summary>
		public static BranchMappingExpression OnSubBranch(
			this ICommitMappingExpression exp, string parentMask, int parentMaskOffset = 0)
		{
			return new BranchMappingExpression(exp, parentMask, parentMaskOffset, true);
		}
	}

	public interface IBranchMappingExpression : ICommitMappingExpression
	{}

	public class BranchMappingExpression : EntityMappingExpression<Branch>, IBranchMappingExpression
	{
		public BranchMappingExpression(IRepositoryMappingExpression parentExp)
			: base(parentExp)
		{
			entity = NewBranch(null, 0, true, false);
			Add(entity);
			entity.Commits.Add(CurrentEntity<Commit>());
		}

		public BranchMappingExpression(
			IRepositoryMappingExpression parentExp,
			string mask,
			int maskOffset,
			bool subBranch)
			: base(parentExp)
		{
			entity = Get<Branch>()
				.SingleOrDefault(b => b.Mask == mask && b.MaskOffset == maskOffset);
			if (entity == null)
			{
				entity = NewBranch(mask, maskOffset, subBranch, subBranch);
				Add(entity);
			}
			else
			{
				if (subBranch)
				{
					entity = NewBranch(mask, maskOffset, true, true);
					Add(entity);
				}
			}
			entity.Commits.Add(CurrentEntity<Commit>());
		}
		
		private Branch NewBranch(string mask, int maskOffset, bool createMask, bool combineMask)
		{
			var newBranch = new Branch()
			{
				Mask = mask,
				MaskOffset = maskOffset
			};

			// create a new branch mask
			if (createMask)
			{
				var branchWithMaxMask = GetReadOnly<Branch>()
					.OrderByDescending(x => x.Mask.Length + x.MaskOffset)
					.FirstOrDefault();
				newBranch.Mask = string.Format("{0}1",
					branchWithMaxMask == null ? string.Empty
					: new String('0', branchWithMaxMask.Mask.Length + branchWithMaxMask.MaskOffset));
				newBranch.MaskOffset = 0;
				
				// combine masks for subbranch
				if (combineMask)
				{
					newBranch.CombineMask(mask, maskOffset);
				}
			}
			
			return newBranch;
		}
	}
}
