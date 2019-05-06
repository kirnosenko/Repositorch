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

		public RepositorySelectionExpression(IRepository repository)
		{
			this.repository = repository;
		}
		public IQueryable<T> Queryable<T>() where T : class
		{
			return repository.Get<T>();
		}
		public IQueryable<T> Selection<T>() where T : class
		{
			return Queryable<T>();
		}
	}
}
