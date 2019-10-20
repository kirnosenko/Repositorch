using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace Repositorch.Data.Entities.Persistent
{
	public abstract class NamedDataStore : IDataStore
	{
		private static readonly ILoggerFactory loggerFactory = new LoggerFactory(
			new[] { new DebugLoggerProvider() });
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
		public bool Logging
		{
			get; set;
		}

		protected abstract void Configure(DbContextOptionsBuilder options);

		private ISession GetNewSession()
		{
			return new PersistentSession(c =>
			{
				Configure(c);
				if (Logging)
				{
					c.EnableSensitiveDataLogging();
					c.UseLoggerFactory(loggerFactory);
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
