using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using Repositorch.Data.Entities;
using Repositorch.Web.Projects;

namespace Repositorch.Web.Handlers.Project.Export
{
	public class ExportProjectQueryHandler : IRequestHandler<ExportProjectQuery>
	{
		private readonly IProjectManager projectManager;

		public ExportProjectQueryHandler(IProjectManager projectManager)
		{
			this.projectManager = projectManager;
		}

		public async Task Handle(ExportProjectQuery request, CancellationToken cancellationToken)
		{
			var settings = projectManager.GetProject(request.ProjectName);
			var store = projectManager.GetProjectDataStore(settings);
			using (var s = store.OpenSession())
			{
				var data = new ProjectData()
				{
					Settings = settings,
					Commits = s.GetReadOnly<Commit>().OrderBy(x => x.Number).ToArray(),
					CommitAttributes = s.GetReadOnly<CommitAttribute>().OrderBy(x => x.Id).ToArray(),
					Authors = s.GetReadOnly<Author>().OrderBy(x => x.Id).ToArray(),
					Branches = s.GetReadOnly<Branch>().OrderBy(x => x.Id).ToArray(),
					Files = s.GetReadOnly<CodeFile>().OrderBy(x => x.Id).ToArray(),
					Modifications = s.GetReadOnly<Modification>().OrderBy(x => x.Id).ToArray(),
					Blocks = s.GetReadOnly<CodeBlock>().OrderBy(x => x.Id).ToArray(),
				};

				JsonSerializer serializer = new JsonSerializer();
				serializer.Formatting = Formatting.Indented;
				serializer.NullValueHandling = NullValueHandling.Ignore;

				using (StreamWriter streamWriter = new StreamWriter(request.Output, Encoding.UTF8))
				using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
				{
					serializer.Serialize(jsonWriter, data);
				}
				await request.Output.FlushAsync();
			}
		}
	}
}
