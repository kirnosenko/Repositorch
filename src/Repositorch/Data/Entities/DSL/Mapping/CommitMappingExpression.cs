﻿using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class CommitMappingExtension
	{
		public static CommitMappingExpression AddCommit(this IRepositoryMappingExpression exp, string revision)
		{
			return new CommitMappingExpression(exp, revision);
		}
		public static CommitMappingExpression Commit(this IRepositoryMappingExpression exp, string revision)
		{
			return new CommitMappingExpression(
				exp,
				exp.Get<Commit>().Single(x =>
					x.Revision == revision
				)
			);
		}
	}

	public interface ICommitMappingExpression : IRepositoryMappingExpression
	{}

	public class CommitMappingExpression : EntityMappingExpression<Commit>, ICommitMappingExpression
	{
		public CommitMappingExpression(IRepositoryMappingExpression parentExp, string revision)
			: base(parentExp)
		{
			entity = new Commit();
			entity.OrderedNumber = Get<Commit>().Count() + 1;
			entity.Revision = revision;
			AddEntity();
		}
		public CommitMappingExpression(IRepositoryMappingExpression parentExp, Commit commit)
			: base(parentExp)
		{
			entity = commit;
		}
		public CommitMappingExpression WithMessage(string message)
		{
			entity.Message = message;
			return this;
		}
		public CommitMappingExpression At(DateTime date)
		{
			entity.Date = date;
			return this;
		}
	}
}
