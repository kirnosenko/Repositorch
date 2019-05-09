using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities
{
	public class SqlCeDataStore : IDataStore
	{
		private string fileName;

		public SqlCeDataStore(string fileName)
		{
			this.fileName = fileName;
		}
		ISession IDataStore.OpenSession(bool readOnly)
		{
			return new EfcSession(c =>
			{
				c.UseSqlCe(string.Format("Data Source={0}", fileName));
			})
			{
				ReadOnly = readOnly,
			};
		}
	}
}
