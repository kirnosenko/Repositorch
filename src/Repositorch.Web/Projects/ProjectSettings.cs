using System.IO;
using LiteDB;
using Repositorch.Data.Entities.Mapping;

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
		public bool FastMergeProcessing { get; set; }
		public VcsDataMapper.CheckMode CheckMode { get; set; }

		public string GetFullRepositoryPath()
		{
			return Path.Combine(
				EnvironmentExtensions.GetRepoPath(),
				RepositoryPath);
		}
	}
}
