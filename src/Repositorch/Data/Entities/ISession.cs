using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Data store unit of work abstraction.
	/// </summary>
	public interface ISession : IRepository, IDisposable
	{
		void SubmitChanges();
	}
}
