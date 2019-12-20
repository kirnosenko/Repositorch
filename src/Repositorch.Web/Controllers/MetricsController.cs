using System;
using Autofac.Features.Indexed;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities.Persistent;

namespace Repositorch.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class MetricsController : ControllerBase
	{
		private IIndex<string, IMetric> metrics;
		private IDataStore data;
		
		public MetricsController(IIndex<string,IMetric> metrics)
		{
			this.metrics = metrics;
			data = new SqlServerDataStore("git");
		}

		[HttpGet]
		[Route("Calculate/{name}")]
		public ActionResult<JObject> Calculate([FromRoute]string name)
		{
			return Calculate(name, null);
		}

		[HttpPost]
		[Route("Calculate/{name}")]
		public ActionResult<JObject> Calculate([FromRoute]string name, [FromBody]JObject input)
		{
			var metric = metrics[name];
			var result = metric.Calculate(data, input);

			return Ok(result);
		}
	}
}
