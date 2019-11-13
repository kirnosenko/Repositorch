using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Data.Entities.Mapping
{
	public class ModificationMapper : Mapper<ICodeFileMappingExpression,IModificationMappingExpression>
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

			if (log.IsMerge)
			{
				if (touchedFile == null || touchedFile.Action == TouchedFileAction.MODIFIED)
				{
					return SingleExpression(expression.Modified());
				}

				var branchMask = expression.CurrentEntity<Branch>().Mask;
				var commitsToLookAt = expression.SelectionDSL()
					.Commits().OnBranchBack(branchMask);
				var lastFileModification =
					(from f in expression.Get<CodeFile>().Where(x => x.Path == filePath)
					 join m in expression.Get<Modification>() on f.Id equals m.FileId
					 join c in commitsToLookAt on m.CommitId equals c.Id
					 orderby c.OrderedNumber descending
					 select m).First();
				if (touchedFile != null && touchedFile.Action != lastFileModification.Action)
				{
					if (touchedFile.Action == TouchedFileAction.ADDED)
					{
						return SingleExpression(expression.Added());
					}

					return SingleExpression(expression.Removed());
				}

				return NoExpressions();
			}

			switch (touchedFile.Action)
			{
				case TouchedFileAction.ADDED:
					if (touchedFile.SourcePath == null)
					{
						return Enumerable.Repeat(expression.Added(), 1);
					}
					if (touchedFile.SourceRevision == null)
					{
						var branchMask = expression.CurrentEntity<Branch>().Mask;
						touchedFile.SourceRevision = expression.SelectionDSL()
							.Commits().OnBranchBack(branchMask)
							.OrderByDescending(x => x.OrderedNumber).First().Revision;
					}
					return Enumerable.Repeat(expression
						.CopiedFrom(touchedFile.SourcePath, touchedFile.SourceRevision), 1);
				case TouchedFileAction.MODIFIED:
					return Enumerable.Repeat(expression.Modified(), 1);
				case TouchedFileAction.REMOVED:
					return Enumerable.Repeat(expression.Removed(), 1);
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
