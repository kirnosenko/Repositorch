using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data
{
	public interface ISession : IDisposable
	{
		IQueryable<T> Get<T>(bool readOnly = false) where T : class;
		void Add<T>(T entity) where T : class;
		void AddRange<T>(IEnumerable<T> entities) where T : class;
		void Delete<T>(T entity) where T : class;
		void SubmitChanges();
	}
}
