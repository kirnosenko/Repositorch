using System;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CommitMapper : EntityMapper<IRepositoryMappingExpression,ICommitMappingExpression>
	{
		public CommitMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<ICommitMappingExpression> Map(IRepositoryMappingExpression expression)
		{
			return new CommitMappingExpression[]
			{
				ExpressionFor(expression)
			};
		}
		protected virtual CommitMappingExpression ExpressionFor(IRepositoryMappingExpression expression)
		{
			Log log = vcsData.Log(expression.Revision);

			return expression.AddCommit(log.Revision)
				.At(log.Date)
				.WithMessage(log.Message);
		}
	}
}
