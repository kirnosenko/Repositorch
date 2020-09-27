using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace Repositorch.Web
{
	public class MetricMenuTest
	{
		private Type type1;
		private Type type2;
		private Type type3;
		
		public MetricMenuTest()
		{
			type1 = typeof(Metrics.Summary);
			type2 = typeof(Metrics.Files);
			type3 = typeof(Metrics.Charts.LOC.Summary);
		}

		[Fact]
		public void Should_get_metric_path_and_root_path()
		{
			Assert.Equal("/", MetricMenu.GetMetricRootPath(type1));
			Assert.Equal("/Summary", MetricMenu.GetMetricPath(type1));

			Assert.Equal("/", MetricMenu.GetMetricRootPath(type2));
			Assert.Equal("/Files", MetricMenu.GetMetricPath(type2));

			Assert.Equal("/Charts/LOC", MetricMenu.GetMetricRootPath(type3));
			Assert.Equal("/Charts/LOC/Summary", MetricMenu.GetMetricPath(type3));
		}

		[Fact]
		public void Should_create_metric_menu()
		{
			var menus = MetricMenu.GetMetricMenu(new Type[] { type1, type2, type3 });

			menus.Count
				.Should().Be(2);
			menus.Select(x => x.Key)
				.Should().BeEquivalentTo(new string[] { "/", "/Charts/LOC" });
			menus["/"].Select(x => x.Path)
				.Should().BeEquivalentTo(new string[] { "/Summary", "/Files" });
			menus["/Charts/LOC"].Select(x => x.Path)
				.Should().BeEquivalentTo(new string[] { "/Charts/LOC/Summary" });
		}

		[Fact]
		public void Should_make_metric_menu_linked()
		{
			var menus = MetricMenu.GetLinkedMetricMenu(new Type[] { type1, type2, type3 });

			menus.Count
				.Should().Be(3);
			menus.Select(x => x.Key)
				.Should().BeEquivalentTo(new string[] { "/", "/Charts", "/Charts/LOC" });
			menus["/"].Select(x => x.Path)
				.Should().BeEquivalentTo(new string[] { "/Summary", "/Files", "/Charts" });
			menus["/Charts"].Select(x => x.Path)
				.Should().BeEquivalentTo(new string[] { "/Charts/LOC", "/" });
			menus["/Charts/LOC"].Select(x => x.Path)
				.Should().BeEquivalentTo(new string[] { "/Charts/LOC/Summary", "/Charts" });
		}
	}
}
