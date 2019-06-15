using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositorch.Data.Entities.EF
{
	public class InMemoryDataStore : IDataStore, ILoggerFactory
	{
		private string name;

		public InMemoryDataStore(string name)
		{
			this.name = name;
		}
		public ISession OpenSession()
		{
			return new EfSession(c => c
				.UseInMemoryDatabase(name)
				//.UseLoggerFactory(this)
			);
		}
		
		void IDisposable.Dispose()
		{
		}
		void ILoggerFactory.AddProvider(ILoggerProvider provider)
		{
		}
		ILogger ILoggerFactory.CreateLogger(string categoryName)
		{
			return new InMemoryDataLogger(text =>
			{
				if (text.Contains("Compiling query model:"))
				{
					Debug.WriteLine(text);
				}
			});
		}		
	}
}
