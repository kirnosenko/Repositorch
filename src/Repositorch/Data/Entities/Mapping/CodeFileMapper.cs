using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeFileMapper : EntityMapper<ICommitMappingExpression,ICodeFileMappingExpression>
	{
		public CodeFileMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<ICodeFileMappingExpression> Map(ICommitMappingExpression expression)
		{
			List<string> allTouchedFiles = new List<string>();

			string revision = expression.CurrentEntity<Commit>().Revision;
			Log log = vcsData.Log(revision);
			var logTouchedFiles = log.TouchedFiles;
			if (vcsData.IsMerge(revision))
			{
				var currentBranch = expression.CurrentEntity<Branch>();
				var existentFiles = expression.SelectionDSL()
					.Commits().OnBranchBack(currentBranch.Mask)
					.Files().TouchedInAndStillExistAfterCommits().Select(x => x.Path)
					.ToArray();

				var parentRevisions = vcsData.GetRevisionParents(revision).ToArray();
				var parentBranchMasks = expression.SelectionDSL()
					.Commits().RevisionIsIn(parentRevisions)
					.Branches().OfCommits()
					.Select(b => b.Mask).ToArray();
				var commonParentMask = BranchMask.And(parentBranchMasks);

				List<IQueryable<string>> filesTouchedOnBranches = new List<IQueryable<string>>(parentBranchMasks.Length);
				foreach (var parentBranchMask in parentBranchMasks)
				{
					filesTouchedOnBranches.Add(expression.SelectionDSL()
						.Commits().OnBranchBack(BranchMask.Xor(parentBranchMask, commonParentMask))
						.Files().TouchedInAndStillExistAfterCommits().Select(x => x.Path));
				}
				var filesTouchedOnDifferentBranches =
					from f in filesTouchedOnBranches.SelectMany(x => x)
					group f by f into fileCount
					where fileCount.Count() > 1
					select fileCount.Key;

				foreach (var touchedFile in filesTouchedOnDifferentBranches)
				{
					allTouchedFiles.Add(touchedFile);
				}

				foreach (var touchedFile in vcsData.Diff(revision).TouchedFiles)
				{
					allTouchedFiles.Add(touchedFile);
				}

				logTouchedFiles = logTouchedFiles.Where(tf =>
					(tf.Action == TouchedFile.TouchedFileAction.ADDED && existentFiles.All(ef => ef != tf.Path))
					||
					(tf.Action == TouchedFile.TouchedFileAction.REMOVED && existentFiles.Any(ef => ef == tf.Path)));
			}

			foreach (var touchedFile in logTouchedFiles)
			{
				allTouchedFiles.Add(touchedFile.Path);
			}

			IEnumerable<string> filteredFiles = allTouchedFiles;
			if (PathSelectors != null)
			{
				filteredFiles = filteredFiles
					.Where(x => PathSelectors.All(ps => ps.IsSelected(x)));
			}

			return filteredFiles.Distinct().Select(x => expression.File(x)).ToArray();
		}
		public IPathSelector[] PathSelectors
		{
			get; set;
		}
	}
}
