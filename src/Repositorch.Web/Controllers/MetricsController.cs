using System;
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
		private IIndex<string,string[]> metricsMenu;
		
		public MetricsController(
			IIndex<string,IMetric> metrics,
			IIndex<string,string[]> metricsMenu)
		{
			this.metrics = metrics;
			this.metricsMenu = metricsMenu;
		}

		[HttpGet]
		[Route("[action]/{metricPath}")]
		public ActionResult<JObject> GetMenu([FromRoute]string metricPath)
		{
			metricPath = Uri.UnescapeDataString(metricPath);
			if (metricsMenu.TryGetValue(metricPath, out var menu))
			{
				return Ok(menu);
			}
			
			return Ok(Enumerable.Empty<string>());
		}

		[HttpGet]
		[Route("{project}/{metric}")]
		public ActionResult<JObject> Calculate(
			[FromRoute]string project,
			[FromRoute]string metric)
		{
			return Calculate(project, metric, null);
		}

		[HttpPost]
		[Route("{project}/{metric}")]
		public ActionResult<JObject> Calculate(
			[FromRoute]string project, 
			[FromRoute]string metric,
			[FromBody]JObject input)
		{
			var data = new SqlServerDataStore(project);
			var metricToCalculate = metrics[metric];
			var result = metricToCalculate.Calculate(data, input);

			return Ok(result);
		}
	}
}
