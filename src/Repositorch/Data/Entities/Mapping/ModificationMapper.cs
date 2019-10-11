using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;

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
			Lazy<string> fileCheckSum = new Lazy<string>(
				() => vcsData.Blame(commit.Revision, filePath).CheckSum);
			Log log = vcsData.Log(commit.Revision);
			var touchedFile = log.TouchedFiles.SingleOrDefault(x => x.Path == filePath);

			if (touchedFile == null) // file was not taken from log but from history, so this is a merge
			{
				var branchMask = expression.CurrentEntity<Branch>().Mask;
				var commitsToLookAt = expression.SelectionDSL()
					.Commits().OnBranchBack(branchMask);
				var lastFileChecksum =
					(from f in expression.Get<CodeFile>().Where(x => x.Path == filePath)
					 join m in expression.Get<Modification>() on f.Id equals m.FileId
					 join c in commitsToLookAt on m.CommitId equals c.Id
					 orderby c.OrderedNumber descending
					 select m.CheckSum).First();
				if (lastFileChecksum != fileCheckSum.Value)
				{
					return Enumerable.Repeat(expression.Modified().HasCheckSum(fileCheckSum.Value), 1);
				}

				return Enumerable.Empty<IModificationMappingExpression>();
			}

			switch (touchedFile.Action)
			{
				case TouchedFile.TouchedFileAction.ADDED:
					if (touchedFile.SourcePath == null)
					{
						return Enumerable.Repeat(expression.Added().HasCheckSum(fileCheckSum.Value), 1);
					}
					if (touchedFile.SourceRevision == null)
					{
						touchedFile.SourceRevision = expression.Get<Commit>()
							.Single(c => c.OrderedNumber == commit.OrderedNumber - 1)
							.Revision;
					}
					return Enumerable.Repeat(expression
						.CopiedFrom(touchedFile.SourcePath, touchedFile.SourceRevision)
						.HasCheckSum(fileCheckSum.Value), 1);
				case TouchedFile.TouchedFileAction.MODIFIED:
					return Enumerable.Repeat(expression.Modified().HasCheckSum(fileCheckSum.Value), 1);
				case TouchedFile.TouchedFileAction.REMOVED:
					return Enumerable.Repeat(expression.Removed(), 1);
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
