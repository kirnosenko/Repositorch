using System;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public static class DefectDensity
	{
		/// <summary>
		/// Calculates traditional defect density for specified code
		/// as number of defects to total size of remaining code.
		/// </summary>
		public static double CalculateTraditionalDefectDensity(this CodeBlockSelectionExpression code)
		{
			code = code.Added().Fixed();

			return CalculateDefectDensity(
				code.CalculateNumberOfDefects(),
				code.CalculateLOC() + code.ModifiedBy().Removed().CalculateLOC()
			);
		}
		/// <summary>
		/// Calculate traditional defect density for specified code
		/// were added in revision or before it.
		/// </summary>
		public static double CalculateTraditionalDefectDensity(this CodeBlockSelectionExpression code, string revision)
		{
			code = code
				.Commits().TillRevision(revision)
				.CodeBlocks().Again().AddedInitiallyInCommits().Fixed();

			return CalculateDefectDensity(
				code.CalculateNumberOfDefects(revision),
				code.CalculateLOC()
				+
				code
					.Modifications().InCommits()
					.CodeBlocks().Again().ModifiedBy().Removed().InModifications().CalculateLOC()
			);
		}

		/// <summary>
		/// Calculate defect density for specified code
		/// as number of defects to total size of added code.
		/// </summary>
		public static double CalculateDefectDensity(this CodeBlockSelectionExpression code)
		{
			code = code.Added().Fixed();

			return CalculateDefectDensity(
				code.CalculateNumberOfDefects(),
				code.CalculateLOC()
			);
		}
		/// <summary>
		/// Calculate defect density for specified code
		/// were added in revision or before it.
		/// </summary>
		public static double CalculateDefectDensity(this CodeBlockSelectionExpression code, string revision)
		{
			code = code
				.Commits().TillRevision(revision)
				.CodeBlocks().Again().AddedInitiallyInCommits().Fixed();

			return CalculateDefectDensity(
				code.CalculateNumberOfDefects(revision),
				code.CalculateLOC()
			);
		}
		private static double CalculateDefectDensity(double numberOfDefects, double codeSize)
		{
			if (codeSize == 0)
			{
				return 0;
			}
			return numberOfDefects / codeSize;
		}
	}
}
