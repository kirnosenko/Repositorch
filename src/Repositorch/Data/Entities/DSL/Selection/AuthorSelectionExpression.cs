using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public static class AuthorSelectionExtensions
	{
		public static AuthorSelectionExpression Authors(this IRepositorySelectionExpression parentExp)
		{
			return new AuthorSelectionExpression(parentExp);
		}
		public static CommitSelectionExpression AuthorIs(
			this CommitSelectionExpression parentExp,
			string name)
		{
			return parentExp.Reselect(s =>
				from c in s
				join a in parentExp.Queryable<Author>() on c.AuthorId equals a.Id
				where a.Name == name
				select c
			);
		}
		public static CommitSelectionExpression AuthorsAre(
			this CommitSelectionExpression parentExp,
			params string[] names)
		{
			return parentExp.Reselect(s =>
				from c in s
				join a in parentExp.Queryable<Author>() on c.AuthorId equals a.Id
				where names.Contains(a.Name)
				select c
			);
		}
		public static CommitSelectionExpression AuthorIsNot(
			this CommitSelectionExpression parentExp,
			string name)
		{
			return parentExp.Reselect(s =>
				from c in s
				join a in parentExp.Queryable<Author>() on c.AuthorId equals a.Id
				where a.Name != name
				select c
			);
		}
	}

	public class AuthorSelectionExpression : EntitySelectionExpression<Author, AuthorSelectionExpression>
	{
		public AuthorSelectionExpression(IRepositorySelectionExpression parentExp)
			: base(parentExp)
		{
		}
		public AuthorSelectionExpression OfCommits()
		{
			return Reselect((s) =>
				(from c in Selection<Commit>()
				join a in s on c.AuthorId equals a.Id
				select a).Distinct()
			);
		}
		public AuthorSelectionExpression WithName(string name)
		{
			return Reselect((s) =>
				from a in s
				where a.Name == name
				select a
			);
		}
		public AuthorSelectionExpression WithEmail(string email)
		{
			return Reselect((s) =>
				from a in s
				where a.Email == email
				select a
			);
		}
		protected override AuthorSelectionExpression Recreate()
		{
			return new AuthorSelectionExpression(this);
		}
	}
}
