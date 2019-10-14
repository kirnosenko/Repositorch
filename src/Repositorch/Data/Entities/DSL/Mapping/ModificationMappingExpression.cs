using System;
using System.Linq;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public static class ModificationMappingExtension
	{
		public static ModificationMappingExpression Modified(this ICodeFileMappingExpression exp)
		{
			return new ModificationMappingExpression(exp, TouchedFileAction.MODIFIED);
		}
		public static ModificationMappingExpression Added(this ICodeFileMappingExpression exp)
		{
			return new ModificationMappingExpression(exp, TouchedFileAction.ADDED);
		}
		public static ModificationMappingExpression CopiedFrom(
			this ICodeFileMappingExpression exp,
			string sourseFilePath,
			string sourceRevision)
		{
			return new ModificationMappingExpression(exp, sourseFilePath, sourceRevision);
		}
		public static IModificationMappingExpression Removed(this ICodeFileMappingExpression exp)
		{
			return new ModificationMappingExpression(exp, TouchedFileAction.REMOVED);
		}
	}

	public interface IModificationMappingExpression : ICodeFileMappingExpression
	{}

	public class ModificationMappingExpression : EntityMappingExpression<Modification>, IModificationMappingExpression
	{
		public ModificationMappingExpression(
			IRepositoryMappingExpression parentExp,
			TouchedFileAction action)
			: base(parentExp)
		{
			entity = new Modification()
			{
				Action = action,
				Commit = CurrentEntity<Commit>(),
				File = CurrentEntity<CodeFile>(),
			};
			Add(entity);
		}
		public ModificationMappingExpression(
			IRepositoryMappingExpression parentExp,
			string sourseFilePath,
			string sourceRevision)
			: this(parentExp, TouchedFileAction.ADDED)
		{
			entity.SourceCommit = Get<Commit>()
				.Single(x => x.Revision == sourceRevision);
			// use DSL to make additional check that file exists in the revision
			entity.SourceFile = this.SelectionDSL()
				.Files().PathIs(sourseFilePath).ExistInRevision(sourceRevision).Single();
		}

		public override IRepositoryMappingExpression Revert()
		{
			Remove(entity);
			return this;
		}
	}
}
