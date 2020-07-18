using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.Persistent
{
	public class SqlServerDataStore : NamedDataStore
	{
		private readonly string address;
		private readonly string credentials;

		public SqlServerDataStore(
			string name,
			string address,
			string port = null,
			string user = null,
			string password = null)
			: base(name)
		{
			this.address = address + (string.IsNullOrEmpty(port) ? string.Empty : $",{port}");
			this.credentials = (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
				? $"user id={user};password={password}"
				: "Trusted_Connection=True";
		}
		protected override void Configure(DbContextOptionsBuilder options)
		{
			var cs = $"server={address};database={name};{credentials};MultipleActiveResultSets=true;";

			options.UseSqlServer(cs);
		}
	}
}
