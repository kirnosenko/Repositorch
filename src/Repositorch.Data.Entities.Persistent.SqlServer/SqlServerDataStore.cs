using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.Persistent
{
	public class SqlServerDataStore : NamedDataStore
	{
		public SqlServerDataStore(string name)
			: base(name)
		{
		}
		protected override void Configure(DbContextOptionsBuilder options)
		{
			var cs = $"Server=.\\SQLEXPRESS;Database={name};Trusted_Connection=True;";
			
			options.UseSqlServer(cs);
		}
	}
}
