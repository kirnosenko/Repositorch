using System.IO;
using LiteDB;

namespace Repositorch.Web.Projects
{
	public class ProjectSettings
	{
		[BsonId]
		public string Name { get; set; }
		public string StoreName { get; set; }
		public string VcsName { get; set; }
		public string RepositoryPath { get; set; }
		public string Branch { get; set; }
		public bool UseExtendedLog { get; set; }
		public string CheckResult { get; set; }

		public string GetFullRepositoryPath()
		{
			return Path.Combine(
				EnvironmentExtensions.GetRepoPath(),
				RepositoryPath);
		}
		public void Combine(ProjectSettings sourceSettings)
		{
			if (string.IsNullOrEmpty(Name))
			{
				Name = sourceSettings.Name;
			}
			if (string.IsNullOrEmpty(StoreName))
			{
				StoreName = sourceSettings.StoreName;
			}
			if (string.IsNullOrEmpty(VcsName))
			{
				VcsName = sourceSettings.VcsName;
			}
			if (string.IsNullOrEmpty(RepositoryPath))
			{
				RepositoryPath = sourceSettings.RepositoryPath;
			}
			if (string.IsNullOrEmpty(Branch))
			{
				Branch = sourceSettings.Branch;
			}
			UseExtendedLog = UseExtendedLog || sourceSettings.UseExtendedLog;
		}
	}
}
