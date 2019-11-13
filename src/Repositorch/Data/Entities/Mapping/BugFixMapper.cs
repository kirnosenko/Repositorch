using System;
using System.Collections.Generic;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class BugFixMapper : Mapper<ICommitMappingExpression, IBugFixMappingExpression>
	{
		private IBugFixDetector bugFixDetector;

		public BugFixMapper(IVcsData vcsData, IBugFixDetector bugFixDetector)
			: base(vcsData)
		{
			this.bugFixDetector = bugFixDetector;
		}
		public override IEnumerable<IBugFixMappingExpression> Map(ICommitMappingExpression expression)
		{
			if (bugFixDetector.IsBugFix(expression.CurrentEntity<Commit>()))
			{
				return SingleExpression(expression
					.IsBugFix());
			}

			return NoExpressions();
		}
	}
}
