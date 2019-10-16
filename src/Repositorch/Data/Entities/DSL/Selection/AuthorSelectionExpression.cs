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
		public static CommitSelectionExpression ByAuthors(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				from c in s
				join a in parentExp.Selection<Author>() on c.AuthorId equals a.Id
				select c
			);
		}
		public static CommitSelectionExpression NotByAuthors(
			this CommitSelectionExpression parentExp)
		{
			return parentExp.Reselect(s =>
				from c in s
				join a in parentExp.Selection<Author>() on c.AuthorId equals a.Id into agroup
				from author in agroup.DefaultIfEmpty()
				where author == null
				select c
			);
		}
	}

	public class AuthorSelectionExpression : EntitySelectionExpression<Author, AuthorSelectionExpression>
	{
		public AuthorSelectionExpression(
			IRepositorySelectionExpression parentExp,
			IQueryable<Author> selection = null)
			: base(parentExp, selection)
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
		public AuthorSelectionExpression NameIs(string name)
		{
			return Reselect((s) =>
				from a in s
				where a.Name == name
				select a
			);
		}
		public AuthorSelectionExpression NameIsOneOf(params string[] names)
		{
			return Reselect((s) =>
				from a in s
				where names.Contains(a.Name)
				select a
			);
		}
		public AuthorSelectionExpression EmailIs(string email)
		{
			return Reselect((s) =>
				from a in s
				where a.Email == email
				select a
			);
		}

		protected override AuthorSelectionExpression Recreate(IQueryable<Author> selection)
		{
			return new AuthorSelectionExpression(this, selection);
		}
	}
}
