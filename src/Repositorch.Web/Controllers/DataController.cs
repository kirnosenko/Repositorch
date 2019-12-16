using System;
using Autofac.Features.Indexed;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities.Persistent;

namespace Repositorch.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DataController : ControllerBase
	{
		private IIndex<string, IMetric> metrics;
		private IDataStore data;

		public DataController(IIndex<string,IMetric> metrics)
		{
			this.metrics = metrics;
			data = new SqlServerDataStore("git");
		}

		[HttpGet]
		[Route("CalculateMetrics/{name}")]
		public ActionResult<JObject> CalculateMetrics([FromRoute]string name)
		{
			return CalculateMetrics(name, null);
		}

		[HttpPost]
		[Route("CalculateMetrics/{name}")]
		public ActionResult<JObject> CalculateMetrics([FromRoute]string name, [FromBody]JObject input)
		{
			var metric = metrics[name];
			var result = metric.Calculate(data, input);

			return Ok(result);
		}
	}
}
