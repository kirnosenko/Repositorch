using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities.EF
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
			return new EfSession(c => c.UseSqlite(string.Format("Data Source={0}", fileName)));
		}
	}
}
