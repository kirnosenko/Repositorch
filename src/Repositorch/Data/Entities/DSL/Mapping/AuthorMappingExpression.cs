using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class AuthorMappingExtension
	{
		public static AuthorMappingExpression AuthorIs(
			this ICommitMappingExpression exp,
			string name)
		{
			return new AuthorMappingExpression(exp, name);
		}
	}

	public interface IAuthorMappingExpression : ICommitMappingExpression
	{}

	public class AuthorMappingExpression : EntityMappingExpression<Author>, IAuthorMappingExpression
	{
		public AuthorMappingExpression(
			IRepositoryMappingExpression parentExp,
			string name)
			: base(parentExp)
		{
			entity = Get<Author>().SingleOrDefault(a => a.Name == name);
			if (entity == null)
			{
				entity = new Author()
				{
					Name = name,
				};
				Add(entity);
			}
			entity.Commits.Add(CurrentEntity<Commit>());
		}
		public AuthorMappingExpression HasEmail(string email)
		{
			entity.Email = email;
			return this;
		}
	}
}
