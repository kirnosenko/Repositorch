using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Repositorch.Data;
using Repositorch.Data.Entities;
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
			var data = new SqliteDataStore("d:/123.db");
			var gitClient = new CommandLineGitClient("D:/src/git/.git");
			var vcsData = new VcsDataCached(new GitData(gitClient), 1, 1000);
			var mapper = CreateDataMapper(data, vcsData);

			//Selection(data);
			//Dump(vcsData);
			//Map(data, vcsData, 150);
			//Truncate(data, vcsData, 122);
			Check(data, vcsData, 100, false);
			Console.ReadKey();
		}
		static void Selection(IDataStore data)
		{
			using (ConsoleTimeLogger.Start("selection time"))
			{
			
			}
		}
		static void Dump(IVcsData vcsData)
		{
			using (ConsoleTimeLogger.Start("dump time"))
			{
				var r1 = vcsData.GetRevisionByNumber(123);
				var r2 = vcsData.GetRevisionByNumber(124);

				var blame1 = vcsData.Blame(r1, "/Makefile");
				var blame2 = vcsData.Blame(r2, "/Makefile");
				var diff = blame1.Diff(blame2);

				Console.WriteLine();
			}
		}
		static DataMapper CreateDataMapper(IDataStore data, IVcsData vcsData)
		{
			DataMapper dataMapper = new DataMapper(data, vcsData);
			dataMapper.RegisterMapper(
				new CommitMapper(vcsData));
			dataMapper.RegisterMapper(
				new AuthorMapper(vcsData));
			dataMapper.RegisterMapper(
				new BranchMapper(vcsData));
			dataMapper.RegisterMapper(
				new BugFixMapper(vcsData, new BugFixDetectorBasedOnLogMessage()));
			dataMapper.RegisterMapper(
				new CodeFileMapper(vcsData));
			dataMapper.RegisterMapper(
				new ModificationMapper(vcsData));
			dataMapper.RegisterMapper(
				new CodeBlockMapper(vcsData));
			dataMapper.OnError += message =>
			{
				Console.WriteLine(message);
			};
			return dataMapper;
		}
		static void Map(IDataStore data, IVcsData vcsData, int revisions)
		{
			DataMapper mapper = CreateDataMapper(data, vcsData);
			
			using (ConsoleTimeLogger.Start("mapping time"))
			{
				mapper.OnRevisionProcessing += (r, n) => Console.WriteLine(
					"mapping of revision {0}{1}",
					r,
					r != n ? string.Format(" ({0})", n) : ""
				);

				mapper.MapRevisions(stopRevision: vcsData.GetRevisionByNumber(revisions));
			}
		}
		static void Truncate(IDataStore data, IVcsData vcsData, int revisionsToKeep)
		{
			DataMapper mapper = CreateDataMapper(data, vcsData);
			
			using (ConsoleTimeLogger.Start("truncating time"))
			{
				mapper.OnRevisionProcessing += (r, n) => Console.WriteLine(
					"truncating of revision {0}{1}",
					r,
					r != n ? string.Format(" ({0})", n) : ""
				);

				mapper.Truncate(revisionsToKeep);
			}
		}
		static void Check(IDataStore data, IVcsData vcsData, int skipRevisions = 0, bool touchedOnly = true)
		{
			DataMapper mapper = CreateDataMapper(data, vcsData);

			using (ConsoleTimeLogger.Start("checking time"))
			{
				mapper.OnRevisionProcessing += (r, n) => Console.WriteLine(
					"checking of revision {0}{1}",
					r,
					r != n ? string.Format(" ({0})", n) : ""
				);

				var revisions = data.UsingSession(s =>
					s.Get<Commit>().Skip(skipRevisions).Select(c => c.Revision).ToArray());

				foreach (var r in revisions)
				{
					if (!mapper.CheckRevision(r, touchedOnly))
					{
						return;
					}
				}
			}
		}
	}
}
