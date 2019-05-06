using System;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class SessionHelper
	{
		public static RepositoryMappingExpression MappingDSL(this ISession session)
		{
			return new RepositoryMappingExpression(session);
		}
	}
}
