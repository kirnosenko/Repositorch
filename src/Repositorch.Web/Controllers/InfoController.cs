using System.Reflection;
using System.Diagnostics;
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
			Assembly assembly = typeof(Repositorch.Data.IDataStore).Assembly;
			FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
			var version = fileVersionInfo.ProductVersion;
			return Ok(version);
		}

		[Route("[action]")]
		public IActionResult GetVariables()
		{
			return Ok(EnvironmentExtensions.GetVariables());
		}
	}
}
