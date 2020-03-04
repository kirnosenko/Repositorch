using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Repositorch.Data;

namespace Repositorch.Web
{
	public interface IMetric
	{
		object Calculate(IDataStore data, JObject input);
	}

	public abstract class Metric : IMetric
	{
		public virtual object Calculate(IDataStore data, JObject input)
		{
			using (var session = data.OpenSession())
			using (var time = TimeLogger.Start())
			{
				var result = Calculate(session, input);

				return new
				{
					time = time.FormatedTime,
					data = result
				};
			}
		}

		protected abstract object Calculate(ISession session, JObject input);
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
