using System;
using LiteDB;

namespace Repositorch.Web
{
	public class ProjectSettings
	{
		[BsonId]
		public string Name { get; set; }
		public string RepositoryPath { get; set; }
	}
}
