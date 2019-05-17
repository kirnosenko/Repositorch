using System;
using System.Linq;
using System.Diagnostics;
using Repositorch.Data;
using Repositorch.Data.Entities.EF;
using Repositorch.Data.Entities.Mapping;
using Repositorch.Data.VersionControl;
using Repositorch.Data.VersionControl.Git;

namespace Repositorch
{
	class Program
	{
		static void Main(string[] args)
		{
			SqliteDataStore data = new SqliteDataStore("d:/123.db");

			//Selection(data);
			Mapping(data);
			Console.ReadKey();
		}
		static void Selection(IDataStore data)
		{
			using (ConsoleTimeLogger.Start("selection time"))
			{
			
			}
		}
		static void Mapping(IDataStore data)
		{
			IGitClient gitClient = new CommandLineGitClient("D:/src/git/.git");
			IVcsData vcsData = new VcsDataCached(new GitData(gitClient), 1, 1000);

			DataMapper mapping = new DataMapper(vcsData);
			mapping.RegisterMapper(
				new CommitMapper(vcsData));
			mapping.RegisterMapper(
				new AuthorMapper(vcsData));
			mapping.RegisterMapper(
				new BugFixMapper(vcsData, new BugFixDetectorBasedOnLogMessage()));
			mapping.RegisterMapper(
				new CodeFileMapper(vcsData));
			mapping.RegisterMapper(
				new ModificationMapper(vcsData));
			mapping.RegisterMapper(
				new CodeBlockMapper(vcsData));
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
