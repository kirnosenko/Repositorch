using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class ModificationMapper : EntityMapper<ICodeFileMappingExpression,IModificationMappingExpression>
	{
		public ModificationMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<IModificationMappingExpression> Map(ICodeFileMappingExpression expression)
		{
			var commit = expression.CurrentEntity<Commit>();
			var filePath = expression.CurrentEntity<CodeFile>().Path;
			Log log = vcsData.Log(commit.Revision);
			var touchedFile = log.TouchedFiles.SingleOrDefault(x => x.Path == filePath);
			if (touchedFile == null)
			{
				return Result(expression.Modified());
			}

			switch (touchedFile.Action)
			{
				case TouchedFile.TouchedFileAction.ADDED:
					if (touchedFile.SourcePath == null)
					{
						return Result(expression.Added());
					}
					if (touchedFile.SourceRevision == null)
					{
						touchedFile.SourceRevision = expression.Get<Commit>()
							.Single(c => c.OrderedNumber == commit.OrderedNumber - 1)
							.Revision;
					}
					return Result(expression.CopiedFrom(touchedFile.SourcePath, touchedFile.SourceRevision));
				case TouchedFile.TouchedFileAction.MODIFIED:
					return Result(expression.Modified());
				case TouchedFile.TouchedFileAction.REMOVED:
					return Result(expression.Removed());
				default:
					throw new InvalidOperationException();
			}
		}
		private ModificationMappingExpression[] Result(ModificationMappingExpression expression)
		{
			return new ModificationMappingExpression[]
			{
				expression
			};
		}
	}
}
