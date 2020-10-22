using System;
using Xunit;
using FluentAssertions;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public class DefectDensityTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_be_zero_for_empty_code()
		{
			selectionDSL
				.CodeBlocks().CalculateTraditionalDefectDensity()
					.Should().Be(0);
			selectionDSL
				.CodeBlocks().CalculateDefectDensity()
					.Should().Be(0);
		}
		[Fact]
		public void Should_be_zero_for_code_without_bugs()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(15)
			.Submit();

			selectionDSL
				.CodeBlocks().CalculateTraditionalDefectDensity()
					.Should().Be(0);
			selectionDSL
				.CodeBlocks().CalculateDefectDensity()
					.Should().Be(0);
		}
		[Fact]
		public void Traditional_defect_density_is_ratio_of_number_of_defects_to_current_code_size()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
						.Code(3000)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(-100).ForCodeAddedInitiallyInRevision("1")
						.Code(5000)
			.Submit()
				.AddCommit("3").IsBugFix()
					.File("file1").Modified()
						.Code(-2).ForCodeAddedInitiallyInRevision("1")
						.Code(10)
			.Submit()
				.AddCommit("4").IsBugFix()
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("2")
			.Submit()
				.AddCommit("5").IsBugFix()
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("2")
						.Code(10)
			.Submit();

			selectionDSL
				.Commits().RevisionIs("1")
				.Modifications().InCommits()
				.CodeBlocks().InModifications()
				.CalculateTraditionalDefectDensity()
					.Should().Be(1d / (3000 - 100 - 2));
			selectionDSL
				.Commits().RevisionIs("2")
				.Modifications().InCommits()
				.CodeBlocks().InModifications()
				.CalculateTraditionalDefectDensity()
					.Should().Be(2d / (5000 - 5 - 10));
			selectionDSL
				.CodeBlocks()
				.CalculateTraditionalDefectDensity()
					.Should().Be(3d / 7903);
		}
		[Fact]
		public void Defect_density_is_ratio_of_number_of_defects_to_added_code_size()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Modified()
						.Code(3000)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(-100).ForCodeAddedInitiallyInRevision("1")
						.Code(5000)
			.Submit()
				.AddCommit("3").IsBugFix()
					.File("file1").Modified()
						.Code(-2).ForCodeAddedInitiallyInRevision("1")
						.Code(10)
			.Submit()
				.AddCommit("4").IsBugFix()
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("2")
			.Submit()
				.AddCommit("5").IsBugFix()
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("2")
						.Code(10)
			.Submit();

			selectionDSL
				.Commits().RevisionIs("1")
				.Modifications().InCommits()
				.CodeBlocks().InModifications()
				.CalculateDefectDensity()
					.Should().Be(1d / 3000);
			selectionDSL
				.Commits().RevisionIs("2")
				.Modifications().InCommits()
				.CodeBlocks().InModifications()
				.CalculateDefectDensity()
					.Should().Be(2d / 5000);
			selectionDSL
				.CodeBlocks()
				.CalculateDefectDensity()
					.Should().Be(3d / 8020);
		}
		[Fact]
		public void Should_ignore_fixes_after_specified_revision()
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
				.AddCommit("3").OnBranch("1").IsBugFix()
					.File("file1").Modified()
						.Code(-5).ForCodeAddedInitiallyInRevision("1")
						.Code(-1).ForCodeAddedInitiallyInRevision("2")
						.Code(5)
			.Submit();

			var code = selectionDSL
				.Commits().TillRevision("1")
				.Files().PathIs("file1")
				.Modifications().InFiles()
				.CodeBlocks().InModifications();

			code.CalculateTraditionalDefectDensity("1")
				.Should().Be(0);
			code.CalculateTraditionalDefectDensity("2")
				.Should().Be(1d / 100);
			code.CalculateTraditionalDefectDensity("3")
				.Should().Be(2d / 99);

			code.CalculateDefectDensity("1")
				.Should().Be(0);
			code.CalculateDefectDensity("2")
				.Should().Be(1d / 105);
			code.CalculateDefectDensity("3")
				.Should().Be(2d / 110);
		}
	}
}
