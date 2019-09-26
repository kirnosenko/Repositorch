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

				var filesTouchedOnDifferentBranches = GetFilesTouchedOnDifferentBranches(expression, revision);
				
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

		private IEnumerable<string> GetFilesTouchedOnDifferentBranches(ICommitMappingExpression expression, string revision)
		{
			List<string> filesTouchedOnDifferentBranches = new List<string>();

			var parentRevisions = vcsData.GetRevisionParents(revision).ToArray();
			var parentBranchMasks = expression.SelectionDSL()
				.Commits().RevisionIsIn(parentRevisions)
				.Branches().OfCommits()
				.Select(b => b.Mask).ToArray();
			var commonParentMask = BranchMask.And(parentBranchMasks);

			if (parentRevisions.Length == 2) // merge of two branches -> simple case
			{
				var firstBranchMask = BranchMask.Xor(parentBranchMasks[0], commonParentMask);
				var secondBranchMask = BranchMask.Xor(parentBranchMasks[1], commonParentMask);

				filesTouchedOnDifferentBranches.AddRange(
					GetFilesTouchedOnDifferentBranches(expression, firstBranchMask, secondBranchMask));
			}
			else
			{
				var fullParentMask = BranchMask.Or(parentBranchMasks);
				foreach (var parentBranchMask in parentBranchMasks)
				{
					var currentBranchMask = BranchMask.Xor(parentBranchMask, commonParentMask);
					var otherBranchesMask = BranchMask.Xor(currentBranchMask, BranchMask.Xor(commonParentMask, fullParentMask));

					filesTouchedOnDifferentBranches.AddRange(
						GetFilesTouchedOnDifferentBranches(expression, currentBranchMask, otherBranchesMask));
				}
			}

			return filesTouchedOnDifferentBranches;
		}

		private IEnumerable<string> GetFilesTouchedOnDifferentBranches(
			ICommitMappingExpression expression,
			BranchMask singleBranchMask,
			BranchMask otherBranchesMask)
		{
			var filesTouchedOnBranch = expression.SelectionDSL()
				.Commits().OnBranchBack(singleBranchMask)
				.Files().TouchedInAndStillExistAfterCommits().Select(x => x.Path);
			var filesTouchedOnOtherBranches = expression.SelectionDSL()
				.Commits().OnBranchBack(otherBranchesMask)
				.Files().TouchedInAndStillExistAfterCommits().Select(x => x.Path);

			foreach (var file in filesTouchedOnBranch.Intersect(filesTouchedOnOtherBranches))
			{
				yield return file;
			}
		}
	}
}
