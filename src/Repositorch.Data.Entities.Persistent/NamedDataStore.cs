using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.Persistent
{
	public abstract class NamedDataStore : IDataStore
	{
		private ISession session;
		protected readonly string name;

		public NamedDataStore(string name)
		{
			this.name = name;
		}
		public ISession OpenSession()
		{
			return SingletonSession ? GetSession() : GetNewSession();
		}
		public bool SingletonSession
		{
			get; set;
		}
		public Action<string> Logger
		{
			get; set;
		}

		protected abstract void Configure(DbContextOptionsBuilder options);

		private ISession GetNewSession()
		{
			return new PersistentSession(c =>
			{
				Configure(c);
				if (Logger != null)
				{
					c.EnableSensitiveDataLogging();
					c.EnableDetailedErrors();
					c.LogTo(Logger);
				}
			});
		}
		private ISession GetSession()
		{
			if (session == null)
			{
				session = GetNewSession();
			}
			return new SessionWrapper(session);
		}
	}
}
