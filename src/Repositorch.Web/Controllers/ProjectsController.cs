using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Repositorch.Web.Options;
using Repositorch.Web.Projects;

namespace Repositorch.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class ProjectsController : ControllerBase
	{
		private static readonly string GitRepoDirName = ".git";
		private static readonly Regex ProjectNameRegExp = new Regex(@"^[a-zA-Z0-9\-\._\~]*$");
		private readonly IProjectManager projectManager;
		private readonly DataStoreOptionsCollection storeOptions;

		public ProjectsController(
			IProjectManager projectManager,
			IOptions<DataStoreOptionsCollection> storeOptions)
		{
			this.projectManager = projectManager;
			this.storeOptions = storeOptions.Value;
		}

		[HttpGet]
		[Route("[action]/{name}")]
		public IActionResult GetSettings([FromRoute] string name)
		{
			var project = projectManager.GetProject(name);
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
			var names = projectManager.GetProjectNames();

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
			var repoRoot = EnvironmentExtensions.GetRepoPath();
			GetRepoDirs(repoRoot, repoRoot, repoDirs);

			return Ok(repoDirs);
		}

		[HttpPost]
		[Route("[action]")]
		public IActionResult Create()
		{
			var form = HttpContext.Request.Form;
			var settings = JsonConvert.DeserializeObject<ProjectSettings>(form["settings"]);
			ProjectData data = null;

			if (form.Files.Count > 0)
			{
				var stream = new MemoryStream();
				form.Files[0].CopyTo(stream);
				stream.Seek(0, SeekOrigin.Begin);
				data = ProjectData.FromStream(stream);

				settings.Combine(data.Settings);
				data.Settings = settings;
			}

			var errors = Validate(settings, false);
			if (errors.Count == 0)
			{
				projectManager.AddProject(settings);
				if (data != null)
				{
					projectManager.ImportProject(data);
				}
			}

			return Ok(errors);
		}

		[HttpPost]
		[Route("[action]")]
		public IActionResult Update()
		{
			var form = HttpContext.Request.Form;
			var settings = JsonConvert.DeserializeObject<ProjectSettings>(form["settings"]);

			var errors = Validate(settings, true);
			if (errors.Count == 0)
			{
				projectManager.UpdateProject(settings);
			}

			return Ok(errors);
		}

		[HttpDelete]
		[Route("[action]/{name}")]
		public IActionResult Remove([FromRoute] string name)
		{
			projectManager.RemoveProject(name);

			return Ok();
		}

		[HttpGet]
		[Route("[action]/{name}")]
		public IActionResult Export([FromRoute] string name)
		{
			var project = projectManager.GetProject(name);
			if (project == null)
			{
				return BadRequest();
			}
			var data = projectManager.ExportProject(project);
			var stream = data.ToStream();

			return new FileStreamResult(stream, new MediaTypeHeaderValue("application/json"))
			{
				FileDownloadName = $"{name}.json"
			};
		}

		/// <summary>
		/// Return unix-styled relative pathes.
		/// </summary>
		private void GetRepoDirs(string rootDir, string currentDir, List<string> repoDirs)
		{
			var innerDirs = Directory.GetDirectories(currentDir);
			var repoDir = innerDirs.SingleOrDefault(x => x.EndsWith(GitRepoDirName) &&
				new DirectoryInfo(x).Name == GitRepoDirName);

			if (repoDir != null)
			{
				var path = Path.GetRelativePath(rootDir, repoDir).Replace("\\", "/");
				repoDirs.Add(path);
				return;
			}

			foreach (var dir in innerDirs)
			{
				GetRepoDirs(rootDir, dir, repoDirs);
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
				else if (projectManager.GetProject(settings.Name) != null)
				{
					errors.Add(nameof(settings.Name),
						$"Project {settings.Name} already exists.");
				}
			}
			else
			{
				if (projectManager.GetProject(settings.Name) == null)
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
				|| !Directory.Exists(settings.GetFullRepositoryPath()))
			{
				errors.Add(nameof(settings.RepositoryPath),
					"Invalid repository path.");
			}

			if (string.IsNullOrEmpty(settings.Branch))
			{
				errors.Add(nameof(settings.Branch),
					"Invalid branch.");
			}

			if (!update && errors.Count == 0)
			{
				try
				{
					var data = projectManager.GetProjectDataStore(settings);
					using (var s = data.OpenSession())
					{
					}
				}
				catch (Exception e)
				{
					errors.Add(nameof(settings.StoreName),
						$"Invalid data store: {e.Message}");
				}
			}

			return errors;
		}
	}
}
