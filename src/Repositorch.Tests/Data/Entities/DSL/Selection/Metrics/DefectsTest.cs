using System;
using Xunit;
using FluentAssertions;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public class DefectsTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_return_zero_for_empty_code()
		{
			selectionDSL
				.CodeBlocks().CalculateNumberOfDefects()
					.Should().Be(0);
		}
		[Fact]
		public void Should_calc_number_of_fixed_defects_for_code()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2").OnBranch("1").IsBugFix()
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
						.Code(5)
			.Submit()
				.AddCommit("3").OnBranch("1")
					.File("file1").Modified()
						.Code(10)
					.File("file2").Added()
						.Code(50)
			.Submit()
				.AddCommit("4").OnBranch("1").IsBugFix()
					.File("file1").Modified()
						.Code(-1).ForCodeAddedInitiallyInRevision("1")
						.Code(1)
					.File("file2").Modified()
						.Code(-1).ForCodeAddedInitiallyInRevision("3")
						.Code(1)
			.Submit();

			selectionDSL
				.Files().PathIs("file1")
				.Modifications().InFiles()
				.CodeBlocks().InModifications().CalculateNumberOfDefects()
					.Should().Be(2);
			selectionDSL
				.Commits()
				.Files().PathIs("file1")
				.Modifications().InFiles()
				.CodeBlocks().InModifications().CalculateNumberOfDefects("3")
					.Should().Be(1);
			selectionDSL
				.Files().PathIs("file2")
				.Modifications().InFiles()
				.CodeBlocks().InModifications().CalculateNumberOfDefects()
					.Should().Be(1);
		}
	}
}
