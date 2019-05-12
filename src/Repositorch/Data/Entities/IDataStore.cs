using System;

namespace Repositorch.Data.Entities
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

	public interface IDataStore
	{
		ISession OpenSession();
	}
}
