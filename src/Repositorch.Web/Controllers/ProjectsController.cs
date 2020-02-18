using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LiteDB;
using Repositorch.Data.Entities.Persistent;

namespace Repositorch.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class ProjectsController : ControllerBase
    {
		private readonly LiteCollection<ProjectSettings> projects;

		public ProjectsController(LiteDatabase liteDb)
		{
			projects = liteDb.GetCollection<ProjectSettings>();
		}

		[HttpGet]
		[Route("[action]/{name}")]
		public IActionResult GetSettings([FromRoute]string name)
		{
			var project = projects.FindOne(x => x.Name == name);
			if (project == null)
			{
				return BadRequest();
			}

			return Ok(project);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetNames()
		{
			var names = projects.FindAll().Select(x => x.Name);

			return Ok(names);
		}

		[HttpPost]
		[Route("[action]")]
		public IActionResult Create(ProjectSettings settings)
		{
			var project = projects.FindOne(x => x.Name == settings.Name);
			if (project != null)
			{
				return BadRequest();
			}

			projects.Insert(settings);
			return Ok();
		}

		[HttpPost]
		[Route("[action]")]
		public IActionResult Update(ProjectSettings settings)
		{
			var project = projects.FindOne(x => x.Name == settings.Name);
			if (project == null)
			{
				return BadRequest();
			}

			projects.Update(settings);
			return Ok();
		}

		[HttpDelete]
		[Route("[action]/{name}")]
		public IActionResult Remove([FromRoute]string name)
		{
			if (projects.Delete(x => x.Name == name) == 0)
			{
				return BadRequest();
			}
			
			//var data = new SqlServerDataStore(name);
			//using (var session = data.OpenSession())
			{
				//var context = session as DbContext;
				//context.Database.EnsureDeleted();
			}
			
			return Ok();
		}
	}
}
