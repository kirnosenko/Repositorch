using System.Linq;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CommitAttributeMapper : Mapper<ICommitMappingExpression, ICommitAttributeMappingExpression>
	{
		public CommitAttributeMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<ICommitAttributeMappingExpression> Map(
			ICommitMappingExpression expression)
		{
			CommitAttributeMappingExpression resultExp = null;
			var log = vcsData.Log(expression.CurrentEntity<Commit>().Revision);

			if (log.IsMerge)
			{
				resultExp = (resultExp ?? expression).IsMerge();
			}

			if (log.IsSplit)
			{
				resultExp = (resultExp ?? expression).IsSplit();
			}

			if (resultExp != null)
			{
				return SingleExpression(resultExp);
			}

			return NoExpressions();
		}
	}
}
