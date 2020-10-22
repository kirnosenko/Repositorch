using System;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public static class DefectCodeDensity
	{
		/// <summary>
		/// Calculates defect code density for specified code
		/// as defect code size to number of lines.
		/// </summary>
		public static double CalculateDefectCodeDensity(this CodeBlockSelectionExpression code)
		{
			code = code.Added().Fixed();

			return CalculateDefectCodeDensity(
				code.CalculateDefectCodeSize(),
				code.CalculateLOC()
			);
		}
		/// <summary>
		/// Calculates defect code density for specified code
		/// were fixed in revision or before it.
		/// </summary>
		public static double CalculateDefectCodeDensity(this CodeBlockSelectionExpression code, string revision)
		{
			code = code
				.Commits().TillRevision(revision)
				.CodeBlocks().Again().AddedInitiallyInCommits().Fixed();

			return CalculateDefectCodeDensity(
				code.CalculateDefectCodeSize(revision),
				code.CalculateLOC()
			);
		}
		private static double CalculateDefectCodeDensity(double defectCodeSize, double addedCodeSize)
		{
			if (addedCodeSize == 0)
			{
				return 0;
			}
			return defectCodeSize / addedCodeSize;
		}
	}
}
