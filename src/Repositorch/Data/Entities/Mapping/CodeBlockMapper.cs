using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeBlockMapper : Mapper<IModificationMappingExpression,ICodeBlockMappingExpression>
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
				return SingleExpression(expression
					.RemoveCode());
			}

			List<CodeBlockMappingExpression> codeBlockExpressions = new List<CodeBlockMappingExpression>();
			string revision = expression.CurrentEntity<Commit>().Revision;
			CodeFile file = expression.CurrentEntity<CodeFile>();
			var blame = vcsData.Blame(revision, file.Path);

			if (blame != null)
			{
				var linesByRevision = (
					from l in blame
					group l.Key by l.Value into g
					select new
					{
						Revision = g.Key,
						Size = g.Count(),
					}).ToList();

				void CopyCode()
				{
					foreach (var linesForRevision in linesByRevision)
					{
						var newExp = expression.Code(linesForRevision.Size);
						if (linesForRevision.Revision != revision)
						{
							newExp.CopiedFrom(linesForRevision.Revision);
						}
						codeBlockExpressions.Add(newExp);
					}
				};

				if ((modification.Action == TouchedFileAction.ADDED) &&
					(modification.SourceFile != null))
				{
					CopyCode();
				}
				else
				{
					var addedCode = linesByRevision.SingleOrDefault(x => x.Revision == revision);
					if (addedCode != null)
					{
						linesByRevision.Remove(addedCode);
						codeBlockExpressions.Add(
							expression.Code(addedCode.Size)
						);
					}

					var isMerge = vcsData.Log(revision).IsMerge;
					var updatedCodeExp = expression.UpdateCode((exp, rev, size) =>
					{
						var linesForRevision = linesByRevision.SingleOrDefault(x => x.Revision == rev);
						if (linesForRevision != null)
						{
							linesByRevision.Remove(linesForRevision);
						}
						double realCodeSize = linesForRevision == null ? 0 : linesForRevision.Size;
						if ((size > realCodeSize) ||
							(size < realCodeSize && isMerge))
						{
							var newExp = expression.Code(realCodeSize - size);
							newExp.ForCodeAddedInitiallyInRevision(rev);
							return newExp;
						}

						return null;
					});
					if (isMerge && linesByRevision.Count > 0)
					{
						CopyCode();
					}
					if (updatedCodeExp != null)
					{
						codeBlockExpressions.Add(updatedCodeExp);
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
