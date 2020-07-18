using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.Persistent
{
	public class PostgreSqlDataStore : NamedDataStore
	{
		private readonly string address;
		private readonly string credentials;
		
		public PostgreSqlDataStore(
			string name,
			string address,
			string port,
			string user,
			string password)
			: base(name)
		{
			if (string.IsNullOrEmpty(port))
			{
				port = "5432";
			}
			this.address = $"Server={address};Port={port}";
			this.credentials = $"User ID={user};Password={password}";
		}
		protected override void Configure(DbContextOptionsBuilder options)
		{
			var cs = $"{credentials};{address};Database={name};Integrated Security=true;Pooling=true;";

			options.UseNpgsql(cs);
		}
	}
}
