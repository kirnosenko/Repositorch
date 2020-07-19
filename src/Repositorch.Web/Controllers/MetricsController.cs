using System;
using System.Collections.Generic;
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
		private readonly IProjectManager projectFactory;
		private readonly IIndex<string,IMetric> metrics;
		private readonly IIndex<string,List<MetricMenu.MetricMenuItem>> metricsMenu;
		
		public MetricsController(
			IProjectManager projectFactory,
			IIndex<string,IMetric> metrics,
			IIndex<string,List<MetricMenu.MetricMenuItem>> metricsMenu)
		{
			this.projectFactory = projectFactory;
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
		[Route("{project}/{metricPath}")]
		public ActionResult<JObject> Calculate(
			[FromRoute]string project, 
			[FromRoute]string metricPath,
			[FromQuery]IDictionary<string, string> parameters)
		{
			metricPath = Uri.UnescapeDataString(metricPath);
			
			if (metrics.TryGetValue(metricPath, out var metric))
			{
				var data = projectFactory.GetProjectDataStore(project);
				using (var repository = data.OpenSession())
				{
					var settingsObject = parameters.Count > 0
						? parameters.ToDictionary(
							x => x.Key,
							x => x.Value != null ? Uri.UnescapeDataString(x.Value) : null)
						: metric.GetSettings(repository);
					var settings = settingsObject != null
						? JObject.FromObject(settingsObject)
						: null;
					var result = metric.Calculate(repository, settings);

					Dictionary<string, object> metricData = new Dictionary<string, object>()
					{
						{ nameof(result), result }
					};
					if (parameters.Count == 0 && settings != null)
					{
						metricData.Add(nameof(settings), settings);
					}

					return Ok(metricData);
				}
			}

			return BadRequest();
		}
	}
}
