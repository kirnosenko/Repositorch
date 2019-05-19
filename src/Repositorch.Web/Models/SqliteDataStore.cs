using System;
using Microsoft.EntityFrameworkCore;
using Repositorch.Data;
using Repositorch.Data.Entities.EF;

namespace Repositorch.Web.Models
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
