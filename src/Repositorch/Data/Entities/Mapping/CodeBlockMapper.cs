using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeBlockMapper : EntityMapper<IModificationMappingExpression,ICodeBlockMappingExpression>
	{
		public CodeBlockMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}

		public override IEnumerable<ICodeBlockMappingExpression> Map(IModificationMappingExpression expression)
		{
			Modification modification = expression.CurrentEntity<Modification>();

			if (modification.Action == TouchedFileAction.REMOVED)
			{
				return Enumerable.Repeat(expression.RemoveCode(), 1);
			}

			List<CodeBlockMappingExpression> codeBlockExpressions = new List<CodeBlockMappingExpression>();
			string revision = expression.CurrentEntity<Commit>().Revision;
			CodeFile file = expression.CurrentEntity<CodeFile>();
			var blame = vcsData.Blame(revision, file.Path);

			if (blame != null)
			{
				var linesByRevision = from l in blame group l.Key by l.Value;
				bool fileCopied = (modification.Action == TouchedFileAction.ADDED) &&
					(modification.SourceFile != null);

				if (fileCopied)
				{
					foreach (var linesForRevision in linesByRevision)
					{
						var newExp = expression.Code(linesForRevision.Count());
						if (linesForRevision.Key != revision)
						{
							newExp.CopiedFrom(linesForRevision.Key);
						}
						codeBlockExpressions.Add(newExp);
					}
				}
				else
				{
					var addedCode = linesByRevision.SingleOrDefault(x => x.Key == revision);
					if (addedCode != null)
					{
						codeBlockExpressions.Add(
							expression.Code(addedCode.Count())
						);
					}

					var currentBranch = expression.CurrentEntity<Branch>();
					var commitsOnBranch = expression.SelectionDSL().Commits()
						.OnBranchBack(currentBranch.Mask);
					var isMerge = vcsData.IsMerge(revision);

					var existent—odeByRevision = (
						from f in expression.Get<CodeFile>().Where(x => x.Id == file.Id)
						join m in expression.Get<Modification>() on f.Id equals m.FileId
						join c in commitsOnBranch on m.CommitId equals c.Id
						join cb in expression.Get<CodeBlock>() on m.Id equals cb.ModificationId
						join tcb in expression.Get<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
						join tcbc in expression.Get<Commit>() on tcb.AddedInitiallyInCommitId equals tcbc.Id
						group cb.Size by tcbc.Revision into g
						select new
						{
							Revision = g.Key,
							CodeSize = g.Sum()
						}).ToArray();

					foreach (var existentCode in existent—odeByRevision)
					{
						var linesForRevision = linesByRevision.SingleOrDefault(x => x.Key == existentCode.Revision);
						double realCodeSize = linesForRevision == null ? 0 : linesForRevision.Count();
						if ((existentCode.CodeSize > realCodeSize) ||
							(existentCode.CodeSize < realCodeSize && isMerge))
						{
							var newExp = expression.Code(realCodeSize - existentCode.CodeSize);
							newExp.ForCodeAddedInitiallyInRevision(existentCode.Revision);
							codeBlockExpressions.Add(newExp);
						}
					}
				}
			}

			if (codeBlockExpressions.Count == 0 && modification.Action == TouchedFileAction.MODIFIED)
			{
				expression.Revert();
			}
			return codeBlockExpressions;
		}
	}
}
