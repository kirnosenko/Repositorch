using System;

namespace Repositorch.Data
{
	public interface IDataStore
	{
		ISession OpenSession(bool readOnly = false);
	}
}
