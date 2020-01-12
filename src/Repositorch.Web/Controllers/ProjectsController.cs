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
		private LiteDatabase liteDb;

		public ProjectsController(LiteDatabase liteDb)
		{
			this.liteDb = liteDb;
		}

		[HttpGet]
		[Route("GetSettings/{name}")]
		public IActionResult GetSettings([FromRoute]string name)
		{
			var projects = liteDb.GetCollection<ProjectSettings>();
			var project = projects.FindOne(x => x.Name == name);
			if (project == null)
			{
				return BadRequest();
			}

			return Ok(project);
		}

		[HttpGet]
		[Route("GetNames")]
		public IActionResult GetNames()
		{
			var projects = liteDb.GetCollection<ProjectSettings>();
			var names = projects.FindAll().Select(x => x.Name);

			return Ok(names);
		}

		[HttpPost]
		[Route("Create")]
		public IActionResult Create(ProjectSettings settings)
		{
			var projects = liteDb.GetCollection<ProjectSettings>();	
			if (projects.FindOne(x => x.Name == settings.Name) != null)
			{
				return BadRequest();
			}

			projects.Insert(settings);
			return Ok();
		}

		[HttpDelete]
		[Route("Remove/{name}")]
		public IActionResult Remove([FromRoute]string name)
		{
			var projects = liteDb.GetCollection<ProjectSettings>();
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
