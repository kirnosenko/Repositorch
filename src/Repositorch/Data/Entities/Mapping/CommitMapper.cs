using System;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CommitMapper : EntityMapper<Commit, RepositoryMappingExpression, CommitMappingExpression>
	{
		public CommitMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<CommitMappingExpression> Map(RepositoryMappingExpression expression)
		{
			return new CommitMappingExpression[]
			{
				ExpressionFor(expression)
			};
		}
		protected virtual CommitMappingExpression ExpressionFor(RepositoryMappingExpression expression)
		{
			Log log = vcsData.Log(expression.Revision);

			return expression.AddCommit(log.Revision)
				.By(log.Author)
				.At(log.Date)
				.WithMessage(log.Message);
		}
	}
}
