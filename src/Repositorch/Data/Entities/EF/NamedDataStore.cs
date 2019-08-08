using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace Repositorch.Data.Entities.EF
{
	public abstract class NamedDataStore : IDataStore
	{
		private static readonly ILoggerFactory loggerFactory = new LoggerFactory(
			new[] { new DebugLoggerProvider() });
		protected readonly string name;

		public NamedDataStore(string name)
		{
			this.name = name;
		}
		public ISession OpenSession()
		{
			return new EfSession(c =>
			{
				Configure(c);
				if (Logging)
				{
					c.EnableSensitiveDataLogging();
					c.UseLoggerFactory(loggerFactory);
				}
			});
		}
		public bool Logging
		{
			get; set;
		}

		protected abstract void Configure(DbContextOptionsBuilder options);
	}
}
