using System;
using Microsoft.AspNetCore.Mvc;

namespace Repositorch.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class InfoController : ControllerBase
	{
		[Route("[action]")]
		public IActionResult GetVersion()
		{
			var version = typeof(Repositorch.Data.IDataStore).Assembly.GetName().Version;
			return Ok($"{version.Major}.{version.Minor}.{version.Build} alpha");
		}

		[Route("[action]")]
		public IActionResult GetVariables()
		{
			return Ok(EnvironmentExtensions.GetVariables());
		}
	}
}
