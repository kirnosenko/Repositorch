using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class TagMappingExtension
	{
		public static TagMappingExpression HasTag(
			this ICommitMappingExpression exp, string title)
		{
			return new TagMappingExpression(exp, title);
		}
		public static TagMappingExpression HasTags(
			this ICommitMappingExpression exp, params string[] titles)
		{
			TagMappingExpression newExp = null;

			foreach (var title in titles)
			{
				newExp = new TagMappingExpression(newExp ?? exp, title);
			}

			return newExp;
		}
	}

	public interface ITagMappingExpression : ICommitMappingExpression
	{}

	public class TagMappingExpression : EntityMappingExpression<CommitAttribute>, ITagMappingExpression
	{
		public TagMappingExpression(IRepositoryMappingExpression parentExp, string title)
			: base(parentExp)
		{
			entity = new CommitAttribute()
			{
				Type = CommitAttribute.TAG,
				Data = title,
				Commit = CurrentEntity<Commit>()
			};
			Add(entity);
		}
	}
}
