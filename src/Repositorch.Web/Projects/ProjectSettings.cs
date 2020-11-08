using System.IO;
using LiteDB;
using Repositorch.Data.Entities.Mapping;

namespace Repositorch.Web.Projects
{
	public class ProjectSettings : VcsDataMapper.MappingSettings
	{
		[BsonId]
		public string Name { get; set; }
		public string StoreName { get; set; }
		public string VcsName { get; set; }
		public string RepositoryPath { get; set; }
		public string Branch { get; set; }
		public bool UseExtendedLog { get; set; }

		/// <summary>
		/// Last revision in repository on considered branch.
		/// We need to track this to continue mapping after 
		/// repository update.
		/// </summary>
		public string LastRepositoryRevision { get; set; }

		public string GetFullRepositoryPath()
		{
			return Path.Combine(
				EnvironmentExtensions.GetRepoPath(),
				RepositoryPath);
		}
	}
}
