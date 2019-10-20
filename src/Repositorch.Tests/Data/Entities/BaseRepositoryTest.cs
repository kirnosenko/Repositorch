using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.Persistent;

namespace Repositorch.Data.Entities
{
	public abstract class BaseRepositoryTest : ISession
	{
		protected RepositoryMappingExpression mappingDSL;
		protected RepositorySelectionExpression selectionDSL;
		private InMemoryDataStore data;
		private ISession session;

		public BaseRepositoryTest()
		{
			data = new InMemoryDataStore(Guid.NewGuid().ToString());
			session = data.OpenSession();
			mappingDSL = session.MappingDSL();
			selectionDSL = session.SelectionDSL();
		}
		public void Dispose()
		{
			session.Dispose();
		}
		public IQueryable<T> Get<T>() where T : class
		{
			return session.Get<T>();
		}
		public IQueryable<T> GetReadOnly<T>() where T : class
		{
			return session.Get<T>();
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
		public void SubmitChanges()
		{
			session.SubmitChanges();
		}
	}
}
