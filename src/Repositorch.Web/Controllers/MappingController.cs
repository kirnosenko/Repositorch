using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Repositorch.Web.Projects;

namespace Repositorch.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MappingController : ControllerBase
    {
		private MappingService mappingService;

		public MappingController(IEnumerable<IHostedService> hostedServices)
		{
			this.mappingService = (MappingService)hostedServices.First(
				x => x.GetType() == typeof(MappingService));
		}

		[HttpGet]
		[Route("[action]/{path}")]
		public IActionResult CheckValidRepository([FromRoute] string path)
		{
			return Ok(true);
		}

		[HttpPost]
		[Route("[action]/{name}")]
		public IActionResult Start([FromRoute] string name)
		{
			mappingService.StartMapping(name);
			return Ok(true);
		}

		[HttpPost]
		[Route("[action]/{name}")]
		public IActionResult Stop([FromRoute] string name)
		{
			mappingService.StopMapping(name);
			return Ok(true);
		}
	}
}
