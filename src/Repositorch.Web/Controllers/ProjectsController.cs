using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
		private static readonly Regex ProjectNameRegExp = new Regex(@"^[a-zA-Z0-9\-\._\~]*$");
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
			var errors = Validate(settings);
			if (errors.Count == 0)
			{
				projects.Insert(settings);
			}

			return Ok(errors);
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

		private Dictionary<string, string> Validate(ProjectSettings settings)
		{
			var errors = new Dictionary<string, string>();

			if (string.IsNullOrEmpty(settings.Name))
			{
				errors.Add(nameof(settings.Name),
					"Project name may not be empty");
			}
			else if (!ProjectNameRegExp.IsMatch(settings.Name))
			{
				errors.Add(nameof(settings.Name),
					"Project name may consist of letters(A-Z, a-z), digits (0-9) and special characters '-', '.', '_', '~'.");
			}
			else if (projects.FindOne(x => x.Name == settings.Name) != null)
			{
				errors.Add(nameof(settings.Name),
					$"Project {settings.Name} already exists.");
			}

			if (string.IsNullOrEmpty(settings.RepositoryPath))
			{
				errors.Add(nameof(settings.RepositoryPath),
					"Invalid repository path.");
			}

			if (string.IsNullOrEmpty(settings.Branch))
			{
				errors.Add(nameof(settings.Branch),
					"Invalid branch.");
			}

			return errors;
		}
	}
}
