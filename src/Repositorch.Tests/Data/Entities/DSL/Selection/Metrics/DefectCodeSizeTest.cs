using System;
using Xunit;
using FluentAssertions;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public class DefectCodeSizeTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_return_zero_for_empty_code()
		{
			selectionDSL
				.CodeBlocks().CalculateDefectCodeSize()
					.Should().Be(0);

			selectionDSL
				.CodeBlocks().CalculateDefectCodeSizePerDefect()
					.Should().Be(0);
		}
		[Fact]
		public void Should_calc_size_of_defect_code()
		{
			AddCode();

			selectionDSL
				.CodeBlocks().CalculateDefectCodeSize()
					.Should().Be(7);
			selectionDSL
				.Commits().TillRevision("1")
				.Modifications().InCommits()
				.CodeBlocks().InModifications().CalculateDefectCodeSize()
					.Should().Be(6);
			selectionDSL
				.Commits().AfterRevision("1")
				.Modifications().InCommits()
				.CodeBlocks().InModifications().CalculateDefectCodeSize()
					.Should().Be(1);
		}
		[Fact]
		public void Should_calc_size_of_defect_code_in_revision()
		{
			AddCode();

			selectionDSL
				.CodeBlocks().CalculateDefectCodeSize("1")
					.Should().Be(0);
			selectionDSL
				.CodeBlocks().CalculateDefectCodeSize("2")
					.Should().Be(5);
			selectionDSL
				.CodeBlocks().CalculateDefectCodeSize("3")
					.Should().Be(5);
			selectionDSL
				.CodeBlocks().CalculateDefectCodeSize("4")
					.Should().Be(7);
		}
		[Fact]
		public void Should_calc_size_of_defect_code_per_defect()
		{
			AddCode();

			selectionDSL
				.CodeBlocks().CalculateDefectCodeSizePerDefect()
					.Should().Be(3.5);
			selectionDSL
				.Commits().TillRevision("1")
				.Modifications().InCommits()
				.CodeBlocks().InModifications().CalculateDefectCodeSizePerDefect()
					.Should().Be(3);
			selectionDSL
				.Commits().AfterRevision("1")
				.Modifications().InCommits()
				.CodeBlocks().InModifications().CalculateDefectCodeSizePerDefect()
					.Should().Be(1);
		}
		[Fact]
		public void Should_calc_size_of_defect_code_per_defect_in_revision()
		{
			AddCode();

			selectionDSL
				.CodeBlocks().CalculateDefectCodeSizePerDefect("2")
					.Should().Be(5);
			selectionDSL
				.CodeBlocks().CalculateDefectCodeSizePerDefect("3")
					.Should().Be(5);
			selectionDSL
				.CodeBlocks().CalculateDefectCodeSizePerDefect("4")
					.Should().Be(3.5);
		}
		private void AddCode()
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
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(10)
			.Submit()
				.AddCommit("4").OnBranch("1").IsBugFix()
					.File("file1").Modified()
						.Code(-1).ForCodeAddedInitiallyInRevision("1")
						.Code(-1).ForCodeAddedInitiallyInRevision("2")
						.Code(1)
			.Submit();
		}
	}
}
