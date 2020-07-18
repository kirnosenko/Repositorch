using System;
using LiteDB;

namespace Repositorch.Web
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
	}
}
