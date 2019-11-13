﻿using System;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class AuthorMapper : Mapper<ICommitMappingExpression, IAuthorMappingExpression>
	{
		public AuthorMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<IAuthorMappingExpression> Map(ICommitMappingExpression expression)
		{
			Log log = vcsData.Log(expression.CurrentEntity<Commit>().Revision);

			return SingleExpression(expression
				.AuthorIs(log.AuthorName)
				.HasEmail(log.AuthorEmail));
		}
	}
}
