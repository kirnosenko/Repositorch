using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeFileMapper : EntityMapper<CodeFile,CommitMappingExpression,CodeFileMappingExpression>
	{
		public CodeFileMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<CodeFileMappingExpression> Map(CommitMappingExpression expression)
		{
			List<CodeFileMappingExpression> fileExpressions = new List<CodeFileMappingExpression>();

			string revision = expression.CurrentEntity<Commit>().Revision;
			Log log = vcsData.Log(revision);
			var touchedFiles = FilterTouchedFiles(log.TouchedFiles);
			
			foreach (var touchedFile in touchedFiles)
			{
				CodeFileMappingExpression fileExp = null;
				
				switch (touchedFile.Action)
				{
					case TouchedFile.TouchedFileAction.MODIFIED:
						fileExp = expression.File(touchedFile.Path);
						break;
					case TouchedFile.TouchedFileAction.ADDED:
						fileExp = expression.AddFile(touchedFile.Path);
						break;
					case TouchedFile.TouchedFileAction.DELETED:
						fileExp = expression.File(touchedFile.Path);
						fileExp.Delete();
						break;
					default:
						break;
				}
				if (touchedFile.SourcePath != null)
				{
					if (touchedFile.SourceRevision == null)
					{
						touchedFile.SourceRevision = expression.Get<Commit>()
							.Single(c => c.OrderedNumber == expression.CurrentEntity<Commit>().OrderedNumber - 1)
							.Revision;
					}
					fileExp.CopiedFrom(touchedFile.SourcePath, touchedFile.SourceRevision);
				}

				fileExpressions.Add(fileExp);
			}

			return fileExpressions;
		}
		public IPathSelector[] PathSelectors
		{
			get; set;
		}
		private IEnumerable<TouchedFile> FilterTouchedFiles(IEnumerable<TouchedFile> touchedFiles)
		{
			if (PathSelectors != null)
			{
				touchedFiles = touchedFiles
					.Where(tf => PathSelectors.All(ps => ps.IsSelected(tf.Path)));
			}
			return touchedFiles;
		}
	}
}
