﻿using System;
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
		private IIndex<string,IMetric> metrics;
		private IIndex<string,List<object>> metricsMenu;
		
		public MetricsController(
			IIndex<string,IMetric> metrics,
			IIndex<string,List<object>> metricsMenu)
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
				var metricRootPath = metric.GetType().GetMetricRootPath();
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
			[FromRoute]string metricPath)
		{
			return Calculate(project, metricPath, null);
		}

		[HttpPost]
		[Route("{project}/{metricPath}")]
		public ActionResult<JObject> Calculate(
			[FromRoute]string project, 
			[FromRoute]string metricPath,
			[FromBody]JObject input)
		{
			metricPath = Uri.UnescapeDataString(metricPath);

			if (metrics.TryGetValue(metricPath, out var metric))
			{
				var data = new SqlServerDataStore(project);
				var result = metric.Calculate(data, input);

				return Ok(result);
			}

			return BadRequest();
		}
	}
}
