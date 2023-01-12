using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Repositorch.Web.Handlers.Project.GetDataFromForm;
using Repositorch.Web.Handlers.Project.Export;
using Repositorch.Web.Handlers.Project.Import;
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
		private readonly IMediator mediator;
		private readonly IProjectManager projectManager;
		private readonly DataStoreOptionsCollection storeOptions;

		public ProjectsController(
			IMediator mediator,
			IProjectManager projectManager,
			IOptions<DataStoreOptionsCollection> storeOptions)
		{
			this.mediator = mediator;
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
			GetRepoDirs(repoRoot, repoRoot, repoDirs, 0, 1);

			return Ok(repoDirs);
		}

		[HttpPost]
		[Route("[action]")]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> Create()
		{
			var cancellationToken = HttpContext.RequestAborted;
			var form = HttpContext.Request.Form;
			var settings = JsonConvert.DeserializeObject<ProjectSettings>(form["settings"]);
			var data = await mediator.Send(new GetProjectDataFromFormQuery()
			{
				Settings = settings,
				File = form.Files.Count > 0 ? form.Files[0].OpenReadStream() : null
			}, cancellationToken);
			
			var errors = Validate(settings, false);
			if (errors.Count == 0)
			{
				projectManager.AddProject(settings);
				if (data != null)
				{
					await mediator.Send(new ImportProjectCommand()
					{
						Data = data
					}, cancellationToken);
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
		public async Task Export([FromRoute] string name)
		{
			var cancellationToken = HttpContext.RequestAborted;
			HttpContext.Response.ContentType = "application/json";
			HttpContext.Response.StatusCode = 200;
			await HttpContext.Response.StartAsync(cancellationToken);
			await mediator.Send(new ExportProjectQuery()
			{
				ProjectName = name,
				Output = HttpContext.Response.BodyWriter.AsStream(),
			}, cancellationToken);
		}

		/// <summary>
		/// Return unix-styled relative pathes.
		/// </summary>
		private void GetRepoDirs(string rootDir, string currentDir, List<string> repoDirs, int depth, int depthMax)
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

			if (depth < depthMax)
			{
				foreach (var dir in innerDirs)
				{
					GetRepoDirs(rootDir, dir, repoDirs, depth + 1, depthMax);
				}
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
