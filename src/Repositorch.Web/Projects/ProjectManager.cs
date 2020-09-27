using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using LiteDB;
using Repositorch.Data;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.Persistent;
using Repositorch.Data.VersionControl.Git;
using Repositorch.Web.Options;

namespace Repositorch.Web.Projects
{
	public class ProjectManager : IProjectManager
	{
		private readonly ILiteCollection<ProjectSettings> projects;
		private readonly DataStoreOptionsCollection options;

		public ProjectManager(
			LiteDatabase liteDb,
			IOptions<DataStoreOptionsCollection> options)
		{
			this.projects = liteDb.GetCollection<ProjectSettings>();
			this.options = options.Value;
		}

		public IEnumerable<string> GetProjectNames()
		{
			return projects.FindAll().Select(x => x.Name);
		}

		public ProjectSettings GetProject(string projectName)
		{
			return projects.FindOne(x => x.Name == projectName);
		}

		public void AddProject(ProjectSettings projectSettings)
		{
			projects.Insert(projectSettings);
		}

		public void UpdateProject(ProjectSettings projectSettings)
		{
			projects.Update(projectSettings);
		}

		public void RemoveProject(string projectName)
		{
			var data = GetProjectDataStore(projectName);
			using (var session = data.OpenSession())
			{
				var context = session as DbContext;
				context.Database.EnsureDeleted();
			}

			projects.Delete(new BsonValue(projectName));
		}

		public IDataStore GetProjectDataStore(string projectName)
		{
			var projectSettings = GetProject(projectName);
			if (projectSettings == null)
			{
				throw new ArgumentException(nameof(projectName));
			}

			return GetProjectDataStore(projectSettings);
		}

		public IDataStore GetProjectDataStore(ProjectSettings projectSettings)
		{
			DataStoreOptions projectStoreOptions = options.Store[projectSettings.StoreName];
			switch (projectStoreOptions.DataBase)
			{
				case "sqlserver":
					return new SqlServerDataStore(
						projectSettings.Name,
						projectStoreOptions.Host,
						projectStoreOptions.Port,
						projectStoreOptions.User,
						projectStoreOptions.Password);
				case "postgresql":
					return new PostgreSqlDataStore(
						projectSettings.Name,
						projectStoreOptions.Host,
						projectStoreOptions.Port,
						projectStoreOptions.User,
						projectStoreOptions.Password);
				default:
					throw new ArgumentException(nameof(projectStoreOptions.DataBase));
			}
		}

		public IVcsData GetProjectVcsData(string projectName)
		{
			var projectSettings = GetProject(projectName);
			if (projectSettings == null)
			{
				throw new ArgumentException(nameof(projectName));
			}

			return GetProjectVcsData(projectSettings);
		}

		public IVcsData GetProjectVcsData(ProjectSettings projectSettings)
		{
			var vcsClient = new CommandLineGitClient(projectSettings.GetFullRepositoryPath())
			{
				Branch = projectSettings.Branch,
				ExtendedLog = projectSettings.UseExtendedLog,
			};
			var vcsData = new VcsDataCached(vcsClient, 1000, 1000);

			return vcsData;
		}
	}
}
