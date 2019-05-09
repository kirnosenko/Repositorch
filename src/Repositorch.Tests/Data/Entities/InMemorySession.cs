using System;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities
{
	public class InMemorySession : EfcSession
	{
		public InMemorySession(string name)
			: base(c => c.UseInMemoryDatabase(name))
		{
		}
	}
}
