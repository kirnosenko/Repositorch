using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class BranchMapper : Mapper<ICommitMappingExpression, IBranchMappingExpression>
	{
		public BranchMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<IBranchMappingExpression> Map(ICommitMappingExpression expression)
		{
			var log = vcsData.Log(expression.CurrentEntity<Commit>().Revision);

			if (log.IsOrphan)
			{
				return SingleExpression(expression
					.OnFreshBranch());
			}
			else // one or more parent revisions
			{
				var parentBranches =
					(from c in expression.GetReadOnly<Commit>()
					join b in expression.GetReadOnly<Branch>() on c.BranchId equals b.Id
					where log.ParentRevisions.Contains(c.Revision)
					select b).ToArray();
				var parentChildren = log.ParentRevisions
					.SelectMany(pr => vcsData.Log(pr).ChildRevisions)
					.Distinct().Count();

				if (parentBranches.Length == 1) // single-parent revision
				{
					if (parentChildren == 1)
					{
						return SingleExpression(expression
							.OnBranch(parentBranches[0].Mask));
					}
					else
					{
						return SingleExpression(expression
							.OnSubBranch(parentBranches[0].Mask));
					}
				}
				else // multi-parent revision (merge)
				{
					var combinedMask = BranchMask.Or(
						parentBranches.Select(x => x.Mask).ToArray());
					
					if (parentChildren == 1)
					{
						return SingleExpression(expression
							.OnBranch(combinedMask));
					}
					else
					{
						return SingleExpression(expression
							.OnSubBranch(combinedMask));
					}
				}
			}
		}
	}
}
