using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.Persistent
{
	public class SqlServerDataStore : NamedDataStore
	{
		private string address;
		private string access;

		public SqlServerDataStore(
			string name,
			string address,
			string port = null,
			string user = null,
			string password = null)
			: base(name)
		{
			this.address = address + (string.IsNullOrEmpty(port) ? string.Empty : $",{port}");
			this.access = (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
				? $"user id={user};password={password}"
				: "Trusted_Connection=True";
		}
		protected override void Configure(DbContextOptionsBuilder options)
		{
			//var cs = $"Server=.\\SQLEXPRESS;Database={name};Trusted_Connection=True;MultipleActiveResultSets=true;";

			var cs = $"server={address};database={name};{access};MultipleActiveResultSets=true;";

			options.UseSqlServer(cs);
		}
	}
}
