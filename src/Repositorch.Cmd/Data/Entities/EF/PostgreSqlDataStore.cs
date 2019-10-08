using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.EF
{
	public class PostgreSqlDataStore : NamedDataStore
	{
		private string user;
		private string password;

		public PostgreSqlDataStore(string name, string user, string password)
			: base(name)
		{
			this.user = user;
			this.password = password;
		}
		protected override void Configure(DbContextOptionsBuilder options)
		{
			options.UseNpgsql(
				$"User ID={user};Password={password};Server=localhost;Port=5432;Database={name};Integrated Security=true;Pooling=true;");
		}
	}
}
