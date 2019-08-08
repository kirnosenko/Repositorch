using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.EF
{
	public class SqliteDataStore : NamedDataStore
	{
		public SqliteDataStore(string fileName)
			: base(fileName)
		{
		}
		protected override void Configure(DbContextOptionsBuilder options)
		{
			options.UseSqlite(string.Format("Data Source={0}", name));
		}
	}
}
