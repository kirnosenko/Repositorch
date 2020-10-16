using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeFileMapper : Mapper<ICommitMappingExpression,ICodeFileMappingExpression>
	{
		public CodeFileMapper(IVcsData vcsData)
			: base(vcsData)
		{
			FastMergeProcessing = false;
		}
		public bool FastMergeProcessing
		{
			get; set;
		}

		public override IEnumerable<ICodeFileMappingExpression> Map(ICommitMappingExpression expression)
		{
			var log = vcsData.Log(expression.CurrentEntity<Commit>().Revision);
			var touchedFiles = log.TouchedFiles;

			if (touchedFiles.Any(x => x.Type != TouchedFile.ContentType.UNKNOWN))
			{
				var allFiles = expression.GetReadOnly<CodeFile>()
					.Select(x => x.Path)
					.ToArray();

				touchedFiles = touchedFiles
					.Where(x =>
						allFiles.Contains(x.Path) ||
						(x.Action == TouchedFileAction.ADDED && x.Type == TouchedFile.ContentType.TEXT))
					.ToArray();
			}

			var touchedPathes = touchedFiles.Select(x => x.Path);
			if (log.IsMerge)
			{
				if (!FastMergeProcessing)
				{
					touchedPathes = touchedPathes.Union(
						GetFilesTouchedOnParentBranches(expression, log));
				}
				else
				{
					touchedPathes = touchedFiles
						.Where(x => x.Action != TouchedFileAction.MODIFIED)
						.Select(x => x.Path)
						.Union(GetFilesTouchedOnDifferentParentBranches(expression, log));
				}
			}

			if (PathSelectors != null)
			{
				touchedPathes = touchedPathes
					.Where(x => PathSelectors.All(ps => ps.IsSelected(x)));
			}

			return touchedPathes.Select(x => expression.File(x)).ToArray();
		}
		public IPathSelector[] PathSelectors
		{
			get; set;
		}
		
		private IEnumerable<string> GetFilesTouchedOnParentBranches(
			ICommitMappingExpression expression, Log log)
		{
			var parentBranchMasks = expression.SelectionDSL()
				.Commits().RevisionIsIn(log.ParentRevisions)
				.Branches().OfCommits()
				.Select(b => b.Mask).ToArray();
			var andMask = BranchMask.And(parentBranchMasks);
			var orMask = BranchMask.Or(parentBranchMasks);
			var branchesMask = BranchMask.Xor(andMask, orMask);

			return expression.SelectionDSL()
				.Commits().OnBranchBack(branchesMask)
				.Files().TouchedInAndStillExistAfterCommits()
				.Select(x => x.Path).ToArray();
		}
		private IEnumerable<string> GetFilesTouchedOnDifferentParentBranches(
			ICommitMappingExpression expression, Log log)
		{
			List<string> filesTouchedOnDifferentBranches = new List<string>();

			var parentBranchMasks = expression.SelectionDSL()
				.Commits().RevisionIsIn(log.ParentRevisions)
				.Branches().OfCommits()
				.Select(b => b.Mask).ToArray();
			var commonParentMask = BranchMask.And(parentBranchMasks);

			if (log.ParentRevisions.Count() == 2) // merge of two branches -> simple case
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
