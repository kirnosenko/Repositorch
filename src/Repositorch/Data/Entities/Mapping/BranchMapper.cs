using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class BranchMapper : EntityMapper<ICommitMappingExpression, IBranchMappingExpression>
	{
		public BranchMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<IBranchMappingExpression> Map(ICommitMappingExpression expression)
		{
			var parentRevisions = vcsData.GetRevisionParents(
				expression.CurrentEntity<Commit>().Revision).ToArray();

			if (parentRevisions.Length == 0) // the very first revision or no parent revision
			{
				return new BranchMappingExpression[]
				{
					expression.OnFreshBranch()
				};
			}
			else // one or more parent revisions
			{
				var parentBranches =
					(from pr in parentRevisions
					join c in expression.Get<Commit>() on pr equals c.Revision
					join b in expression.Get<Branch>() on c.BranchId equals b.Id
					select b).ToArray();
				var parentChildren = parentRevisions
					.SelectMany(pr => vcsData.GetRevisionChildren(pr))
					.Distinct().Count();

				if (parentBranches.Length == 1) // single-parent revision
				{
					if (parentChildren == 1)
					{
						return new BranchMappingExpression[]
						{
							expression.OnBranch(
								parentBranches[0].Mask, parentBranches[0].MaskOffset)
						};
					}
					else
					{
						return new BranchMappingExpression[]
						{
							expression.OnSubBranch(
								parentBranches[0].Mask, parentBranches[0].MaskOffset)
						};
					}
				}
				else // multi-parent revision (merge)
				{
					var maxParentOffset = parentBranches.Max(x => x.MaskOffset);
					uint combinedMask = 0;
					foreach (var b in parentBranches)
					{
						combinedMask |= b.Mask >> (int)(maxParentOffset - b.MaskOffset);
					}
					if (parentChildren == 1)
					{
						return new BranchMappingExpression[]
						{
							expression.OnBranch(
								combinedMask, maxParentOffset)
						};
					}
					else
					{
						return new BranchMappingExpression[]
						{
							expression.OnSubBranch(
								combinedMask, maxParentOffset)
						};
					}
				}
			}
		}
	}
}
