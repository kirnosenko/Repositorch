using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class BugFixMapper : EntityMapper<BugFix, CommitMappingExpression, BugFixMappingExpression>
	{
		private IBugFixDetector bugFixDetector;

		public BugFixMapper(IVcsData vcsData, IBugFixDetector bugFixDetector)
			: base(vcsData)
		{
			this.bugFixDetector = bugFixDetector;
		}
		public override IEnumerable<BugFixMappingExpression> Map(CommitMappingExpression expression)
		{
			if (bugFixDetector.IsBugFix(expression.CurrentEntity<Commit>()))
			{
				return new BugFixMappingExpression[]
				{
					expression.IsBugFix()
				};
			}
			return Enumerable.Empty<BugFixMappingExpression>();
		}
	}
}
