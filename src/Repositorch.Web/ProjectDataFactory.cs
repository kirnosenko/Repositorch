using System;
using Microsoft.Extensions.Options;
using LiteDB;
using Repositorch.Data;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.Persistent;
using Repositorch.Data.VersionControl.Git;
using Repositorch.Web.Options;

namespace Repositorch.Web
{
	public interface IProjectDataFactory
	{
		IDataStore GetProjectDataStore(string projectName);
		IVcsData GetProjectVcsData(string projectName);
	}

	public class ProjectDataFactory : IProjectDataFactory
	{
		private readonly ILiteCollection<ProjectSettings> projects;
		private readonly DataStoreOptionsCollection options;

		public ProjectDataFactory(
			LiteDatabase liteDb,
			IOptions<DataStoreOptionsCollection> options)
		{
			this.projects = liteDb.GetCollection<ProjectSettings>();
			this.options = options.Value;
		}

		public IDataStore GetProjectDataStore(string projectName)
		{
			var projectSettings = projects.FindOne(x => x.Name == projectName);
			if (projectSettings == null)
			{
				throw new ArgumentException(nameof(projectName));
			}
			
			DataStoreOptions projectStoreOptions = options.Store["SQL Server"];
			return new SqlServerDataStore(
				projectName,
				projectStoreOptions.Address,
				projectStoreOptions.Port,
				projectStoreOptions.User,
				projectStoreOptions.Password);
		}

		public IVcsData GetProjectVcsData(string projectName)
		{
			var projectSettings = projects.FindOne(x => x.Name == projectName);
			if (projectSettings == null)
			{
				throw new ArgumentException(nameof(projectName));
			}

			var vcsClient = new CommandLineGitClient(projectSettings.RepositoryPath)
			{
				Branch = projectSettings.Branch,
				ExtendedLog = projectSettings.UseExtendedLog,
			};
			var vcsData = new VcsDataCached(vcsClient, 1000, 1000);

			return vcsData;
		}
	}
}
