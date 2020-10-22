using System;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public static class DefectCodeSize
	{
		/// <summary>
		/// Calculates defect code size for specified code
		/// as number of lines removed in bugfix commits.
		/// </summary>
		public static double CalculateDefectCodeSize(this CodeBlockSelectionExpression code)
		{
			return -code
				.Commits().AreBugFixes()
				.Modifications().InCommits()
				.CodeBlocks().Again().ModifiedBy().InModifications().CalculateLOC();
		}
		/// <summary>
		/// Calculate defect code size for specified code
		/// were fixed in revision or before it.
		/// </summary>
		public static double CalculateDefectCodeSize(this CodeBlockSelectionExpression code, string revision)
		{
			return -code
				.Commits().AreBugFixes().TillRevision(revision)
				.Modifications().InCommits()
				.CodeBlocks().Again().ModifiedBy().InModifications().CalculateLOC();
		}

		/// <summary>
		/// Calculate average defect code size per defect for specified code.
		/// </summary>
		public static double CalculateDefectCodeSizePerDefect(this CodeBlockSelectionExpression code)
		{
			code = code.Fixed();

			int numberOfDefects = code.CalculateNumberOfDefects();

			return numberOfDefects > 0 ?
				code.CalculateDefectCodeSize() / numberOfDefects
				:
				0;
		}
		/// <summary>
		/// Calculate average defect code size per defect for specified code
		/// were fixed in revision or before it.
		/// </summary>
		public static double CalculateDefectCodeSizePerDefect(this CodeBlockSelectionExpression code, string revision)
		{
			code = code.Fixed();

			int numberOfDefects = code.CalculateNumberOfDefects(revision);

			return numberOfDefects > 0 ?
				code.CalculateDefectCodeSize(revision) / numberOfDefects
				:
				0;
		}
	}
}
