using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities
{
	public class SqliteDataStore : IDataStore
	{
		private string fileName;
		
		public SqliteDataStore(string fileName)
		{
			this.fileName = fileName;
		}
		public ISession OpenSession()
		{
			return new EfcSession(c => c.UseSqlite(string.Format("Data Source={0}", fileName)));
		}
	}
}
