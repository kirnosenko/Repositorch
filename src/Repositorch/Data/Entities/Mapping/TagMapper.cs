using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class TagMapper : Mapper<ICommitMappingExpression, ITagMappingExpression>
	{
		public TagMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<ITagMappingExpression> Map(ICommitMappingExpression expression)
		{
			var log = vcsData.Log(expression.CurrentEntity<Commit>().Revision);
			
			if (log.Tags.Count() == 0)
			{
				return NoExpressions();
			}

			return SingleExpression(expression.HasTags(log.Tags.ToArray()));
		}
	}
}
