using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public interface IRepositorySelectionExpression
	{
		IQueryable<T> Queryable<T>() where T : class;
		IQueryable<T> Selection<T>() where T : class;
	}

	public class RepositorySelectionExpression : IRepositorySelectionExpression
	{
		private IRepository repository;
		private bool mutable;

		public RepositorySelectionExpression(IRepository repository, bool mutable)
		{
			this.repository = repository;
			this.mutable = mutable;
		}
		public IQueryable<T> Queryable<T>() where T : class
		{
			return mutable ? repository.Get<T>() : repository.GetReadOnly<T>();
		}
		public IQueryable<T> Selection<T>() where T : class
		{
			return Queryable<T>();
		}
	}
}
