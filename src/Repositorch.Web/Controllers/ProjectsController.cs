using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		public ActionResult<JObject> GetSettings([FromRoute]string name)
		{
			var projects = liteDb.GetCollection<ProjectSettings>();
			var project = projects.FindOne(x => x.Name == name);

			return Ok(project);
		}

		[HttpGet]
		[Route("GetNames")]
		public ActionResult<JObject> GetNames()
		{
			var projects = liteDb.GetCollection<ProjectSettings>();
			var names = projects.FindAll().Select(x => x.Name);

			return Ok(names);
		}

		[HttpPost]
		[Route("Create")]
		public ActionResult<JObject> Create([FromBody]ProjectSettings settings)
		{
			var projects = liteDb.GetCollection<ProjectSettings>();
			if (projects.FindOne(x => x.Name == settings.Name) != null)
			{
				return BadRequest();
			}

			settings.RepositoryPath = "D:/src/git/.git";
			projects.Insert(settings);
			return Ok();
		}

		[HttpDelete]
		[Route("Remove/{name}")]
		public ActionResult Remove([FromRoute]string name)
		{
			var projects = liteDb.GetCollection<ProjectSettings>();
			projects.Delete(x => x.Name == name);

			var data = new SqlServerDataStore(name);
			using (var session = data.OpenSession())
			{
				var context = session as DbContext;
				context.Database.EnsureDeleted();
			}

			return Ok();
		}
	}
}
