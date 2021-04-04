using System;
using System.Collections;
using System.Linq;
using Xunit;
using FluentAssertions;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Selection
{
	public class CommitSliceTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_select_day_slices()
		{
			mappingDSL
				.AddCommit("1").At(new DateTime(2020, 1, 10))
			.Submit()
				.AddCommit("2").At(new DateTime(2020, 1, 12))
			.Submit()
				.AddCommit("3").At(new DateTime(2020, 1, 13))
			.Submit();

			var commits = GetReadOnly<Commit>().ToArray();
			var slices = this.GetDaySlices();

			slices.Select(x => x.Label)
				.Should().BeEquivalentTo(new string[] { "2020-01-10", "2020-01-11", "2020-01-12", "2020-01-13" });
			slices.Select(x => commits.Where(x.Check.Compile()).Count())
				.Should().BeEquivalentTo(new int[] { 1, 0, 1, 1 });
		}

		[Fact]
		public void Should_select_week_slices()
		{
			mappingDSL
				.AddCommit("1").At(new DateTime(2019, 12, 30))
			.Submit()
				.AddCommit("2").At(new DateTime(2020, 1, 3))
			.Submit()
				.AddCommit("3").At(new DateTime(2020, 1, 7))
			.Submit()
				.AddCommit("4").At(new DateTime(2020, 1, 8))
			.Submit();

			var commits = GetReadOnly<Commit>().ToArray();
			var slices = this.GetWeekSlices(DayOfWeek.Monday);

			slices.Select(x => x.Label)
				.Should().BeEquivalentTo(new string[] { "2019-53", "2020-01", "2020-02" });
			slices.Select(x => commits.Where(x.Check.Compile()).Count())
				.Should().BeEquivalentTo(new int[] { 1, 1, 2 });
		}

		[Fact]
		public void Should_select_month_slices()
		{
			mappingDSL
				.AddCommit("1").At(new DateTime(2019, 12, 31))
			.Submit()
				.AddCommit("2").At(new DateTime(2020, 1, 1))
			.Submit()
				.AddCommit("3").At(new DateTime(2020, 1, 10))
			.Submit()
				.AddCommit("4").At(new DateTime(2020, 3, 20))
			.Submit();

			var commits = GetReadOnly<Commit>().ToArray();
			var slices = this.GetMonthSlices();

			slices.Select(x => x.Label)
				.Should().BeEquivalentTo(new string[] { "2019-12", "2020-01", "2020-02", "2020-03" });
			slices.Select(x => commits.Where(x.Check.Compile()).Count())
				.Should().BeEquivalentTo(new int[] { 1, 2, 0, 1 });
		}

		[Fact]
		public void Should_select_year_slices()
		{
			mappingDSL
				.AddCommit("1").At(new DateTime(2018, 12, 31))
			.Submit()
				.AddCommit("2").At(new DateTime(2020, 1, 1))
			.Submit()
				.AddCommit("3").At(new DateTime(2020, 12, 31))
			.Submit()
				.AddCommit("4").At(new DateTime(2021, 1, 1))
			.Submit()
				.AddCommit("5").At(new DateTime(2021, 12, 31))
			.Submit();

			var commits = GetReadOnly<Commit>().ToArray();
			var slices = this.GetYearSlices();

			slices.Select(x => x.Label)
				.Should().BeEquivalentTo(new string[] { "2018", "2019", "2020", "2021" });
			slices.Select(x => commits.Where(x.Check.Compile()).Count())
				.Should().BeEquivalentTo(new int[] { 1, 0, 2, 2 });
		}
	}
}
