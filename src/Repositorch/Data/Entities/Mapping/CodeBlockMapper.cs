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
			List<CodeBlockMappingExpression> codeBlockExpressions = new List<CodeBlockMappingExpression>();
			string revision = expression.CurrentEntity<Commit>().Revision;
			CodeFile file = expression.CurrentEntity<CodeFile>();
			Modification modification = expression.CurrentEntity<Modification>();
			
			if (modification.Action == Modification.FileAction.REMOVED)
			{
				codeBlockExpressions.Add(
					expression.DeleteCode()
				);
			}
			else
			{
				IBlame blame = vcsData.Blame(revision, file.Path);
				var linesByRevision = from l in blame group l.Key by l.Value;
				bool fileCopied = (modification.Action == Modification.FileAction.ADDED) &&
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
						.OnBranchBack(currentBranch.Mask, currentBranch.MaskOffset);
					foreach (var existentCode in (
						from f in expression.Get<CodeFile>().Where(x => x.Id == file.Id)
						join m in expression.Get<Modification>() on f.Id equals m.FileId
						join c in commitsOnBranch on m.CommitId equals c.Id
						join cb in expression.Get<CodeBlock>() on m.Id equals cb.ModificationId
							let addedCodeID = cb.Size < 0 ? cb.TargetCodeBlockId : cb.Id
							let addedCodeRevision = expression.Get<Commit>()
								.Single(x => x.Id == expression.Get<CodeBlock>()
									.Single(y => y.Id == addedCodeID).AddedInitiallyInCommitId
								).Revision
						group cb.Size by addedCodeRevision into g
						select new
						{
							Revision = g.Key,
							CodeSize = g.Sum()
						}
					))
					{
						var linesForRevision = linesByRevision.SingleOrDefault(x => x.Key == existentCode.Revision);
						double realCodeSize = linesForRevision == null ? 0 : linesForRevision.Count();
						if (existentCode.CodeSize > realCodeSize)
						{
							var newExp = expression.Code(realCodeSize - existentCode.CodeSize);
							newExp.ForCodeAddedInitiallyInRevision(existentCode.Revision);
							codeBlockExpressions.Add(newExp);
						}
						else if (existentCode.CodeSize < realCodeSize && vcsData.IsMerge(revision))
						{
							var newExp = expression
								.Code(realCodeSize - existentCode.CodeSize);
							newExp.CopiedFrom(existentCode.Revision);
							codeBlockExpressions.Add(newExp);	
						}
					}
				}
			}
			
			return codeBlockExpressions;
		}
	}
}
