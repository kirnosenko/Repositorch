using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class EntityMappingExpression<E> : IRepositoryMappingExpression where E : class
	{
		protected E entity;
		private IRepositoryMappingExpression parentExp;

		public EntityMappingExpression(IRepositoryMappingExpression parentExp)
		{
			this.parentExp = parentExp;
		}
		public IQueryable<T> Get<T>() where T : class
		{
			return parentExp.Get<T>();
		}
		public IQueryable<T> GetReadOnly<T>() where T : class
		{
			return parentExp.GetReadOnly<T>();
		}
		public void Add<T>(T entity) where T : class
		{
			parentExp.Add(entity);
		}
		public void AddRange<T>(IEnumerable<T> entities) where T : class
		{
			parentExp.AddRange(entities);
		}
		public void Remove<T>(T entity) where T : class
		{
			parentExp.Remove(entity);
		}
		public void RemoveRange<T>(IEnumerable<T> entities) where T : class
		{
			parentExp.RemoveRange(entities);
		}
		public IRepositoryMappingExpression Submit()
		{
			return parentExp.Submit();
		}
		public virtual T CurrentEntity<T>() where T : class
		{
			if (typeof(T) == typeof(E))
			{
				return entity as T;
			}
			return parentExp.CurrentEntity<T>();
		}
		public string Revision
		{
			get { return parentExp.Revision; }
		}
	}
}
