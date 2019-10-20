using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.Persistent
{
	public class LocalDbDataStore : NamedDataStore
	{
		public LocalDbDataStore(string name)
			: base(name)
		{
		}
		protected override void Configure(DbContextOptionsBuilder options)
		{
			var cs = $"Server=(localdb)\\mssqllocaldb;Database={name};Trusted_Connection=True;";

			options.UseSqlServer(cs);
		}
	}
}
