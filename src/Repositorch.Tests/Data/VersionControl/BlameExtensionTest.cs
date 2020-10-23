using System;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public class BlameExtensionTest
	{
		[Fact]
		public void Should_get_empty_diff_for_equal_blames()
		{
			var blame1 = new TestBlame()
				.AddLinesFromRevision("1", 100)
				.AddLinesFromRevision("2", 50);
			var blame2 = new TestBlame()
				.AddLinesFromRevision("2", 50)
				.AddLinesFromRevision("1", 100);

			blame1.Diff(blame2)
				.Should().BeEmpty();
		}
		[Fact]
		public void Should_get_diff_for_different_blames()
		{
			var blame1 = new TestBlame()
				.AddLinesFromRevision("1", 100)
				.AddLinesFromRevision("2", 50)
				.AddLinesFromRevision("3", 30)
				.AddLinesFromRevision("4", 10);
			var blame2 = new TestBlame()
				.AddLinesFromRevision("1", 100)
				.AddLinesFromRevision("2", 55)
				.AddLinesFromRevision("3", 20)
				.AddLinesFromRevision("5", 5);

			blame1.Diff(blame2)
				.Should().BeEquivalentTo(new Dictionary<string, double>()
				{
					{ "2", +5 },
					{ "3", -10 },
					{ "4", -10 },
					{ "5", +5 },
				});
			blame2.Diff(blame1)
				.Should().BeEquivalentTo(new Dictionary<string, double>()
				{
					{ "2", -5 },
					{ "3", +10 },
					{ "4", +10 },
					{ "5", -5 },
				});
		}
	}
}
