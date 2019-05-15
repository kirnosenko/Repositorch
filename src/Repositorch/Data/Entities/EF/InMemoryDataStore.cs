using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.EF
{
	public class InMemoryDataStore : IDataStore
	{
		private string name;

		public InMemoryDataStore(string name)
		{
			this.name = name;
		}
		public ISession OpenSession()
		{
			return new EfSession(c => c.UseInMemoryDatabase(name));
		}
	}
}
