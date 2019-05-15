using System;

namespace Repositorch.Data
{
	public static class DataStoreExtension
	{
		public static Result UsingSession<Result>(this IDataStore data, Func<ISession,Result> action)
		{
			using (var session = data.OpenSession())
			{
				return action(session);
			}
		}
	}

	/// <summary>
	/// Abstraction of persistent data store.
	/// </summary>
	public interface IDataStore
	{
		ISession OpenSession();
	}
}
