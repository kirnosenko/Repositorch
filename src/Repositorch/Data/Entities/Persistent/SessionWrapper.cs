using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.Entities.Persistent
{
	public class SessionWrapper : ISession
	{
		private ISession innerSession;

		public SessionWrapper(ISession innerSession)
		{
			this.innerSession = innerSession;
		}
		public void Dispose()
		{
			innerSession = null;
		}

		public void Add<T>(T entity) where T : class
		{
			innerSession.Add(entity);
		}
		public void AddRange<T>(IEnumerable<T> entities) where T : class
		{
			innerSession.AddRange(entities);
		}
		public IQueryable<T> Get<T>() where T : class
		{
			return innerSession.Get<T>();
		}
		public IQueryable<T> GetReadOnly<T>() where T : class
		{
			return innerSession.GetReadOnly<T>();
		}
		public void Remove<T>(T entity) where T : class
		{
			innerSession.Remove(entity);
		}
		public void RemoveRange<T>(IEnumerable<T> entities) where T : class
		{
			innerSession.RemoveRange(entities);
		}
		public void SubmitChanges()
		{
			innerSession.SubmitChanges();
		}
	}
}
