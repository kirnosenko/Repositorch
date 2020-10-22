using System;
using System.Linq;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public static class Defects
	{
		/// <summary>
		/// Calculate number of fixed bugs in specified code
		/// </summary>
		public static int CalculateNumberOfDefects(this CodeBlockSelectionExpression code)
		{
			return code.ModifiedBy()
				.Modifications()
					.ContainCodeBlocks()
				.Commits()
					.ContainModifications()
					.AreBugFixes()
					.Count();
		}
		/// <summary>
		/// Calculate number of fixed bugs were fixed in revision or before it.
		/// </summary>
		public static int CalculateNumberOfDefects(this CodeBlockSelectionExpression code, string revision)
		{
			return code.ModifiedBy()
				.Modifications().ContainCodeBlocks()
				.Commits()
					.TillRevision(revision)
					.ContainModifications()
					.AreBugFixes()
					.Count();
		}
	}
}
