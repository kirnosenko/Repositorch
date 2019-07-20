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
			string revision = expression.CurrentEntity<Commit>().Revision;
			string filePath = expression.CurrentEntity<CodeFile>().Path;
			Log log = vcsData.Log(revision);
			var touchedFile = log.TouchedFiles.Single(x => x.Path == filePath);

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
							.Single(c => c.OrderedNumber == expression.CurrentEntity<Commit>().OrderedNumber - 1)
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
