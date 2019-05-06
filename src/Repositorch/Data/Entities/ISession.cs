using System;

namespace Repositorch.Data.Entities
{
	public interface ISession : IRepository, IDisposable
	{
		void SubmitChanges();
	}
}
