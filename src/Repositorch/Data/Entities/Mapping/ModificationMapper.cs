using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class ModificationMapper : EntityMapper<Modification,CodeFileMappingExpression,ModificationMappingExpression>
	{
		public ModificationMapper(IVcsData vcsData)
			: base(vcsData)
		{
		}
		public override IEnumerable<ModificationMappingExpression> Map(CodeFileMappingExpression expression)
		{
			return new ModificationMappingExpression[]
			{
				expression.Modified()
			};
		}
	}
}
