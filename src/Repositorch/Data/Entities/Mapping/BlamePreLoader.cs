using System;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class BlamePreLoader : Mapper<IModificationMappingExpression, IModificationMappingExpression>
	{
		public BlamePreLoader(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<IModificationMappingExpression> Map(IModificationMappingExpression expression)
		{
			Modification modification = expression.CurrentEntity<Modification>();
			if (modification.Action != TouchedFileAction.REMOVED)
			{
				var revision = expression.CurrentEntity<Commit>().Revision;
				var filePath = expression.CurrentEntity<CodeFile>().Path;
				var blame = vcsData.Blame(revision, filePath);
			}

			return SingleExpression(expression);
		}
		public override bool AllowParallel
		{
			get { return true; }
		}
	}
}
