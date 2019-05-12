using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Data.Entities
{
	public class BaseRepositoryTest : IDataStore, ISession
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
		}
		public ISession OpenSession(bool readOnly)
		{
			return this;
		}
		public IQueryable<T> Get<T>() where T : class
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
		public void Delete<T>(T entity) where T : class
		{
			session.Delete(entity);
		}
		public void SubmitChanges()
		{
			session.SubmitChanges();
		}
	}
}
