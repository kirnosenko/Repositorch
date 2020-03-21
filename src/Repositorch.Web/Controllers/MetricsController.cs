using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac.Features.Indexed;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Repositorch.Data.Entities.Persistent;

namespace Repositorch.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class MetricsController : ControllerBase
	{
		private IIndex<string,IMetric> metrics;
		private IIndex<string,List<MetricMenu.MetricMenuItem>> metricsMenu;
		
		public MetricsController(
			IIndex<string,IMetric> metrics,
			IIndex<string,List<MetricMenu.MetricMenuItem>> metricsMenu)
		{
			this.metrics = metrics;
			this.metricsMenu = metricsMenu;
		}

		[HttpGet]
		[Route("[action]/{path}")]
		public ActionResult<JObject> GetMenu([FromRoute]string path)
		{
			path = Uri.UnescapeDataString(path);
			if (metricsMenu.TryGetValue(path, out var menu))
			{
				return Ok(menu);
			}
			else if (metrics.TryGetValue(path, out var metric))
			{
				var metricRootPath = MetricMenu.GetMetricRootPath(metric.GetType());
				if (metricsMenu.TryGetValue(metricRootPath, out menu))
				{
					return Ok(menu);
				}
			}

			return Ok(Enumerable.Empty<string>());
		}
		
		[HttpGet]
		[Route("[action]/{project}/{metricPath}")]
		public ActionResult<JObject> GetSettings(
			[FromRoute]string project,
			[FromRoute]string metricPath)
		{
			metricPath = Uri.UnescapeDataString(metricPath);

			if (metrics.TryGetValue(metricPath, out var metric))
			{
				var data = new SqlServerDataStore(project);
				using (var repository = data.OpenSession())
				{
					var result = metric.GetSettings(repository);

					return Ok(result);
				}
			}

			return BadRequest();
		}

		[HttpGet]
		[Route("[action]/{project}/{metricPath}")]
		public ActionResult<JObject> Calculate(
			[FromRoute]string project, 
			[FromRoute]string metricPath,
			[FromQuery]IDictionary<string, string> settings)
		{
			metricPath = Uri.UnescapeDataString(metricPath);

			if (metrics.TryGetValue(metricPath, out var metric))
			{
				var data = new SqlServerDataStore(project);
				using (var repository = data.OpenSession())
				using (var time = TimeLogger.Start())
				{
					var result = metric.Calculate(repository, JObject.FromObject(settings));

					return Ok(new
					{
						time = time.FormatedTime,
						data = result
					});
				}
			}

			return BadRequest();
		}
	}
}
