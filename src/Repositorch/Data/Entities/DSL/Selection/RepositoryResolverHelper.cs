﻿using System;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class RepositoryResolverHelper
	{
		public static RepositorySelectionExpression SelectionDSL(
			this IRepository repository,
			bool mutable = false)
		{
			return new RepositorySelectionExpression(repository, mutable);
		}
	}
}
