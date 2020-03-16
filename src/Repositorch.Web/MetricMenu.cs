using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Web
{
	public static class MetricMenu
	{
		public class MetricMenuItem
		{
			public string Name { get; set; }
			public string Path { get; set; }
			public bool IsMetric { get; set; }
		}

		private const string MetricsRoot = "Repositorch.Web.Metrics";

		public static string GetMetricPath(Type metric)
		{
			var rootPath = GetMetricRootPath(metric);
			if (rootPath != "/")
			{
				return $"{rootPath}/{metric.Name}";
			}
			return $"/{metric.Name}";
		}
		public static string GetMetricRootPath(Type metric)
		{
			return metric.Namespace == MetricsRoot
				? "/"
				: metric.Namespace
					.Replace(MetricsRoot, "")
					.Replace(".", "/");
		}
		public static Dictionary<string, List<MetricMenuItem>> GetMetricMenu(
			IEnumerable<Type> metrics)
		{
			Dictionary<string, List<MetricMenuItem>> menus = 
				new Dictionary<string, List<MetricMenuItem>>();

			foreach (var metric in metrics)
			{
				var metricPath = GetMetricPath(metric);
				var metricRootPath = GetMetricRootPath(metric);

				if (!menus.ContainsKey(metricRootPath))
				{
					menus.Add(metricRootPath, new List<MetricMenuItem>());
				}
				menus[metricRootPath].Add(new MetricMenuItem()
				{
					Name = metric.Name,
					Path = metricPath,
					IsMetric = true
				});
			}

			return menus;
		}
		public static Dictionary<string, List<MetricMenuItem>> GetLinkedMetricMenu(
			IEnumerable<Type> metrics)
		{
			var menus = GetMetricMenu(metrics);
			var parentMenus = menus.Keys
				.SelectMany(x => ParentPaths(x))
				.ToArray();
			foreach (var m in parentMenus)
			{
				menus.Add(m, new List<MetricMenuItem>());
			}
			var rootPaths = new Stack<string>(menus.Keys
				.OrderBy(x => x.Length));

			while (rootPaths.Count > 1)
			{
				var path = rootPaths.Pop();
				var root = rootPaths
					.Where(x => path.StartsWith(x))
					.OrderByDescending(x => x.Length)
					.FirstOrDefault();
				if (root != null)
				{
					menus[root].Add(new MetricMenuItem()
					{
						Name = root != "/"
							? path.Replace(root + '/', string.Empty)
							: path.Replace("/", string.Empty),
						Path = path,
						IsMetric = false
					});
					menus[path].Add(new MetricMenuItem()
					{
						Path = root,
						IsMetric = false
					});
				}
			}

			return menus;
		}

		private static IEnumerable<string> ParentPaths(string path)
		{
			while ((path = path.Remove(path.LastIndexOf("/"))) != string.Empty)
			{
				yield return path;
			}
		}
	}
}
