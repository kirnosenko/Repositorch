using System;
using Newtonsoft.Json.Linq;
using Repositorch.Data;

namespace Repositorch.Web
{
	public interface IMetric
	{
		object Calculate(IDataStore data, JObject input);
	}

	public static class MetricsExtensions
	{
		private const string MetricsRoot = "Repositorch.Web.Metrics";

		public static string GetMetricPath(this Type metric)
		{
			var rootPath = metric.GetMetricRootPath();
			if (rootPath != "/")
			{
				return $"{rootPath}/{metric.Name}";
			}
			return $"/{metric.Name}";
		}
		public static string GetMetricRootPath(this Type metric)
		{
			return metric.Namespace == MetricsRoot
				? "/"
				: metric.Namespace
					.Replace(MetricsRoot, "")
					.Replace(".", "/");
		}
	}
}
