using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data
{
	/// <summary>
	/// Abstraction to access entities in data store.
	/// </summary>
	public interface IRepository
	{
		IQueryable<T> Get<T>() where T : class;
		IQueryable<T> GetReadOnly<T>() where T : class;
		void Add<T>(T entity) where T : class;
		void AddRange<T>(IEnumerable<T> entities) where T : class;
		void Remove<T>(T entity) where T : class;
		void RemoveRange<T>(IEnumerable<T> entities) where T : class;
	}
}
