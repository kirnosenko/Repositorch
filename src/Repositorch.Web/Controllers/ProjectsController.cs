using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LiteDB;
using Repositorch.Web.Options;

namespace Repositorch.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class ProjectsController : ControllerBase
    {
		private static readonly Regex ProjectNameRegExp = new Regex(@"^[a-zA-Z0-9\-\._\~]*$");
		private readonly ILiteCollection<ProjectSettings> projects;
		private readonly DataStoreOptionsCollection storeOptions;

		public ProjectsController(
			LiteDatabase liteDb,
			IOptions<DataStoreOptionsCollection> storeOptions)
		{
			this.projects = liteDb.GetCollection<ProjectSettings>();
			this.storeOptions = storeOptions.Value;
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

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetDataStoreNames()
		{
			return Ok(storeOptions.Store.Keys);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetRepoDirs()
		{
			List<string> repoDirs = new List<string>();
			GetRepoDirs(EnvironmentExtensions.GetSrcPath(), repoDirs);

			return Ok(repoDirs);
		}

		[HttpPost]
		[Route("[action]")]
		public IActionResult Create(ProjectSettings settings)
		{
			var errors = Validate(settings, false);
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

			var errors = Validate(settings, true);
			if (errors.Count == 0)
			{
				projects.Update(settings);
			}

			return Ok(errors);
		}

		[HttpDelete]
		[Route("[action]/{name}")]
		public IActionResult Remove([FromRoute]string name)
		{
			if (projects.Delete(new BsonValue(name)))
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

		private void GetRepoDirs(string rootDir, List<string> repoDirs)
		{
			var info = new DirectoryInfo(rootDir);
			if (info.Name == ".git")
			{
				repoDirs.Add(rootDir);
				return;
			}

			foreach (var dir in Directory.GetDirectories(rootDir))
			{
				GetRepoDirs(dir, repoDirs);
			}
		}

		private Dictionary<string, string> Validate(ProjectSettings settings, bool update)
		{
			var errors = new Dictionary<string, string>();

			if (!update)
			{
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
			}
			else
			{
				if (projects.FindOne(x => x.Name == settings.Name) == null)
				{
					errors.Add(nameof(settings.Name),
						$"Project {settings.Name} does not exist.");
				}
			}

			if (!storeOptions.Store.TryGetValue(settings.StoreName, out _))
			{
				errors.Add(nameof(settings.StoreName),
					"Invalid data store name.");
			}

			if (string.IsNullOrEmpty(settings.RepositoryPath) 
				|| !Directory.Exists(settings.RepositoryPath))
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
