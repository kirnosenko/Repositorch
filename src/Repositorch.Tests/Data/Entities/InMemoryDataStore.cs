using System;

namespace Repositorch.Data.Entities
{
	public class InMemoryDataStore : IDataStore
	{
		private string name;

		public InMemoryDataStore(string name)
		{
			this.name = name;
		}
		public ISession OpenSession(bool readOnly = false)
		{
			return new InMemorySession(name);
		}
	}
}
