using System;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CommitMapper : Mapper<IRepositoryMappingExpression,ICommitMappingExpression>
	{
		public CommitMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<ICommitMappingExpression> Map(IRepositoryMappingExpression expression)
		{
			Log log = vcsData.Log(expression.Revision);

			return SingleExpression(expression
				.AddCommit(log.Revision)
				.At(log.Date)
				.WithMessage(log.Message));
		}
	}
}
