using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public interface IRepositoryMappingExpression : IRepository
	{
		IRepositoryMappingExpression Submit();
		T CurrentEntity<T>() where T : class;
		string Revision { get; }
	}

	public class RepositoryMappingExpression : IRepositoryMappingExpression
	{
		private ISession session;

		public RepositoryMappingExpression(ISession session)
		{
			this.session = session;
		}
		public IQueryable<T> Get<T>() where T : class
		{
			return session.Get<T>();
		}
		public IQueryable<T> GetReadOnly<T>() where T : class
		{
			return session.GetReadOnly<T>();
		}
		public void Add<T>(T entity) where T : class
		{
			session.Add(entity);
		}
		public void AddRange<T>(IEnumerable<T> entities) where T : class
		{
			session.AddRange(entities);
		}
		public void Remove<T>(T entity) where T : class
		{
			session.Remove(entity);
		}
		public void RemoveRange<T>(IEnumerable<T> entities) where T : class
		{
			session.RemoveRange(entities);
		}
		public IRepositoryMappingExpression Submit()
		{
			session.SubmitChanges();
			return session.MappingDSL();
		}
		public T CurrentEntity<T>() where T : class
		{
			return default(T);
		}
		public string Revision
		{
			get; set;
		}
	}
}
