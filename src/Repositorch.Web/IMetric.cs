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
			if (rootPath != string.Empty)
			{
				return $"{rootPath}/{metric.Name}";
			}
			return metric.Name;
		}
		public static string GetMetricRootPath(this Type metric)
		{
			return metric.Namespace == MetricsRoot
				? string.Empty
				: metric.Namespace
					.Replace(MetricsRoot, "")
					.TrimStart('.')
					.Replace(".", "/")
					.ToLower();
		}
	}
}
