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
				fileExpressions.Add(expression.File(touchedFile.Path));
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
