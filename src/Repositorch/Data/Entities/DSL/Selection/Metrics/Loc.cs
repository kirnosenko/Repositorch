using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public static class Loc
	{
		/// <summary>
		/// Calulate number of lines in specified code.
		/// </summary>
		public static double CalculateLOC(this CodeBlockSelectionExpression code)
		{
			return code.Sum(x => x.Size);
		}
	}
}
