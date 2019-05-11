using System;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class AuthorMapper : EntityMapper<Author, CommitMappingExpression, AuthorMappingExpression>
	{
		public AuthorMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<AuthorMappingExpression> Map(CommitMappingExpression expression)
		{
			Log log = vcsData.Log(expression.CurrentEntity<Commit>().Revision);

			return new AuthorMappingExpression[]
			{
				expression.AuthorIs(log.Author)
			};
		}
	}
}
