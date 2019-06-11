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
			this ICommitMappingExpression exp, uint mask, uint maskOffset = 0)
		{
			return new BranchMappingExpression(exp, mask, maskOffset, false);
		}
		/// <summary>
		/// Create a new sub-branch based on parent branch mask and offset.
		/// </summary>
		public static BranchMappingExpression OnSubBranch(
			this ICommitMappingExpression exp, uint parentMask, uint parentMaskOffset = 0)
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
			if (GetReadOnly<Branch>().Count() == 0)
			{
				entity = NewBranch(1, 0, false, false);
			}
			else
			{
				var branchWithMaxMask = GetBranchWithMaxMask();
				entity = NewBranch(
					branchWithMaxMask.Mask,
					branchWithMaxMask.MaskOffset,
					true,
					false);
			}
			Add(entity);
			entity.Commits.Add(CurrentEntity<Commit>());
		}

		public BranchMappingExpression(
			IRepositoryMappingExpression parentExp,
			uint mask,
			uint maskOffset,
			bool subBranch)
			: base(parentExp)
		{
			entity = Get<Branch>()
				.SingleOrDefault(b => b.Mask == mask && b.MaskOffset == maskOffset);
			if (entity == null)
			{
				if (subBranch)
				{
					entity = NewBranch(mask, maskOffset, true, true);
				}
				else
				{
					entity = NewBranch(mask, maskOffset, false, false);
				}
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

		private Branch GetBranchWithMaxMask()
		{
			var maxOffset = GetReadOnly<Branch>().Max(b => b.MaskOffset);
			return GetReadOnly<Branch>()
				.Where(b => (maxOffset - b.MaskOffset) < Branch.MaskSize)
				.OrderByDescending(b => b.Mask >> (int)(maxOffset - b.MaskOffset))
				.First();
		}
		private Branch NewBranch(uint mask, uint maskOffset, bool createMask, bool combineMask)
		{
			var newBranch = new Branch()
			{
				Mask = mask,
				MaskOffset = maskOffset
			};

			// create a new branch mask
			if (createMask)
			{
				var branchWithMaxMask = GetBranchWithMaxMask();
				int shift = 0;
				while (branchWithMaxMask.Mask != 0)
				{
					shift++;
					branchWithMaxMask.Mask >>= 1;
				}
				if (shift >= Branch.MaskSize)
				{
					throw new InvalidOperationException("Branch mask overflow.");
				}
				newBranch.Mask = 1u << shift;
				newBranch.MaskOffset = branchWithMaxMask.MaskOffset;
			}

			// mask shift when possible
			while ((newBranch.Mask & 3u) == 3u)
			{
				newBranch.Mask >>= 1;
				newBranch.MaskOffset++;
			}

			// combine masks for subbranch
			if (combineMask)
			{
				newBranch.Mask |= mask;
			}

			return newBranch;
		}
	}
}
