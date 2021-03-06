﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public abstract class EntitySelectionExpression<E, Exp> : IRepositorySelectionExpression, IQueryable<E>
		where E : class
		where Exp : class
	{
		private IRepositorySelectionExpression parentExp;
		private IQueryable<E> selection;
		private bool isFixed;

		public EntitySelectionExpression(IRepositorySelectionExpression parentExp, IQueryable<E> selection)
		{
			this.parentExp = parentExp;
			this.selection = selection ?? Queryable<E>();
			isFixed = false;
		}
		
		public IQueryable<T> Queryable<T>() where T : class
		{
			return parentExp.Queryable<T>();
		}
		public IQueryable<T> Selection<T>() where T : class
		{
			if (typeof(T) == typeof(E))
			{
				return (IQueryable<T>)selection;
			}
			return parentExp.Selection<T>();
		}
		public Exp Reselect(Func<Exp, Exp> selector)
		{
			if (selector == null)
			{
				return this as Exp;
			}
			return selector(this as Exp) as Exp;
		}
		public Exp Reselect(Func<IQueryable<E>, IQueryable<E>> selector)
		{
			if (isFixed)
			{
				return Recreate(selector(selection));
			}
			selection = selector(selection);
			return this as Exp;
		}
		public Exp Again()
		{
			return Recreate(parentExp.Selection<E>());
		}
		public Exp Fixed()
		{
			isFixed = true;
			return this as Exp;
		}
		public Exp Do(Action<Exp> action)
		{
			action(this as Exp);
			return this as Exp;
		}
		protected abstract Exp Recreate(IQueryable<E> selection);

		#region IQueryable Members

		public Type ElementType
		{
			get { return selection.ElementType; }
		}

		public System.Linq.Expressions.Expression Expression
		{
			get { return selection.Expression; }
		}

		public IQueryProvider Provider
		{
			get { return selection.Provider; }
		}

		#endregion

		#region IEnumerable<E> Members

		public IEnumerator<E> GetEnumerator()
		{
			return selection.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return selection.GetEnumerator();
		}

		#endregion
	}
}
