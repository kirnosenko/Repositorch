using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using Repositorch.Web.Projects;

namespace Repositorch.Web.Handlers.Project.GetDataFromForm
{
	public class GetProjectDataFromFormQueryHandler : IRequestHandler<GetProjectDataFromFormQuery, ProjectData>
	{
		public async Task<ProjectData> Handle(GetProjectDataFromFormQuery request, CancellationToken cancellationToken)
		{
			ProjectData data = null;

			if (request.File != null)
			{
				var reader = new StreamReader(request.File);
				var jsonReader = new JsonTextReader(reader);
				var serializer = new JsonSerializer();
				data = serializer.Deserialize<ProjectData>(jsonReader);

				CombineSettings(request.Settings, data.Settings);
				data.Settings = request.Settings;

				await request.File.DisposeAsync();
			}

			return data;
		}

		private void CombineSettings(ProjectSettings dest, ProjectSettings src)
		{
			if (string.IsNullOrEmpty(dest.Name))
			{
				dest.Name = src.Name;
			}
			if (string.IsNullOrEmpty(dest.StoreName))
			{
				dest.StoreName = src.StoreName;
			}
			if (string.IsNullOrEmpty(dest.VcsName))
			{
				dest.VcsName = src.VcsName;
			}
			if (string.IsNullOrEmpty(dest.RepositoryPath))
			{
				dest.RepositoryPath = src.RepositoryPath;
			}
			if (string.IsNullOrEmpty(dest.Branch))
			{
				dest.Branch = src.Branch;
			}
			dest.UseExtendedLog = dest.UseExtendedLog || src.UseExtendedLog;
		}
	}
}
