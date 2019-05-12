using System;
using System.Diagnostics;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.Mapping;
using Repositorch.Data.VersionControl;
using Repositorch.Data.VersionControl.Git;

namespace Repositorch
{
	class Program
	{
		static void Main(string[] args)
		{
			Mapping();
			Console.ReadKey();
		}
		static void Mapping()
		{
			string repo = "D:/src/git/.git";

			SqliteDataStore data = new SqliteDataStore("d:/123.db");
			IGitClient gitClient = new CommandLineGitClient(repo);
			IVcsData vcsData = new VcsDataCached(new GitData(gitClient), 1);

			DataMapper mapping = new DataMapper(vcsData);
			var commitMapper = new CommitMapper(vcsData);
			mapping.RegisterMapper(commitMapper);
			var authorMapper = new AuthorMapper(vcsData);
			mapping.RegisterMapper(authorMapper);
			var bugFixMapper = new BugFixMapper(vcsData, new BugFixDetectorBasedOnLogMessage());
			mapping.RegisterMapper(bugFixMapper);
			mapping.CreateDataBase = true;
			mapping.StopRevision = "8f41523fc1a8cd127ff39fa111b3b5bb5105cc84";

			using (ConsoleTimeLogger.Start("mapping time"))
			{
				mapping.OnRevisionMapping += (r, n) => Console.WriteLine(
					"mapping of revision {0}{1}",
					r,
					r != n ? string.Format(" ({0})", n) : ""
				);

				mapping.Map(data);
			}
		}
	}
}
