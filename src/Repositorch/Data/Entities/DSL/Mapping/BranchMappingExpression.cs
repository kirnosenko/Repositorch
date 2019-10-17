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
			this ICommitMappingExpression exp, BranchMask mask)
		{
			return new BranchMappingExpression(exp, mask, false);
		}
		/// <summary>
		/// Create a new sub-branch based on parent branch mask and offset.
		/// </summary>
		public static BranchMappingExpression OnSubBranch(
			this ICommitMappingExpression exp, BranchMask parentMask)
		{
			return new BranchMappingExpression(exp, parentMask, true);
		}
	}

	public interface IBranchMappingExpression : ICommitMappingExpression
	{}

	public class BranchMappingExpression : EntityMappingExpression<Branch>, IBranchMappingExpression
	{
		public BranchMappingExpression(IRepositoryMappingExpression parentExp)
			: base(parentExp)
		{
			entity = NewBranch(null, true, false);
			Add(entity);
			entity.Commits.Add(CurrentEntity<Commit>());
		}

		public BranchMappingExpression(
			IRepositoryMappingExpression parentExp,
			BranchMask mask,
			bool subBranch)
			: base(parentExp)
		{
			entity = Get<Branch>()
				.SingleOrDefault(b => b.Mask.Data == mask.Data && b.Mask.Offset == mask.Offset);
			if (entity == null)
			{
				entity = NewBranch(mask, subBranch, subBranch);
				Add(entity);
			}
			else
			{
				if (subBranch)
				{
					entity = NewBranch(mask, true, true);
					Add(entity);
				}
			}
			entity.Commits.Add(CurrentEntity<Commit>());
		}
		
		private Branch NewBranch(BranchMask mask, bool createMask, bool combineMask)
		{
			var newBranch = new Branch()
			{
				Mask = mask
			};

			// create a new branch mask
			if (createMask)
			{
				var branchWithMaxMask = GetReadOnly<Branch>()
					.OrderByDescending(x => x.Mask.Data.Length + x.Mask.Offset)
					.FirstOrDefault();
				var newMask = new BranchMask()
				{
					Data = string.Format("{0}1",
						branchWithMaxMask == null ? string.Empty
						: new String('0', branchWithMaxMask.Mask.Data.Length + branchWithMaxMask.Mask.Offset)),
					Offset = 0
				};

				// combine masks for subbranch
				if (combineMask)
				{
					newBranch.Mask = BranchMask.Or(newBranch.Mask, newMask);
				}
				else
				{
					newBranch.Mask = newMask;
				}
			}
			
			return newBranch;
		}
	}
}
