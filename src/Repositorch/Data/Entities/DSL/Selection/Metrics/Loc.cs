using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public static class Loc
	{
		public static double CalculateLOC(this CodeBlockSelectionExpression code)
		{
			return code.Sum(x => x.Size);
		}
	}
}
