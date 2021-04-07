using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.Persistent
{
	public class InMemoryDataStore : NamedDataStore
	{
		public InMemoryDataStore(string name)
			: base(name)
		{
		}
		protected override void Configure(DbContextOptionsBuilder options)
		{
			options.UseInMemoryDatabase(name);
		}
	}
}
