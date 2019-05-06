using System;

namespace Repositorch.Data.Entities
{
	public interface IDataStore
	{
		ISession OpenSession(bool readOnly = false);
	}
}
