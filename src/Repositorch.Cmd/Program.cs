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
			
			using (ConsoleTimeLogger.Start("time"))
			{
				Func<DataMapper.MappingSettings> settings = () => new DataMapper.MappingSettings()
				{
					StopRevision = vcsData.GetRevisionByNumber(200),
					Check = DataMapper.CheckMode.ALL,
				};
				mapper.MapRevisions(settings());
				//mapper.Truncate(120);
				//mapper.Check(120, DataMapper.CheckMode.ALL);

				//BlameDiff(vcsData);
			}

			Console.ReadKey();
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
			dataMapper.OnMapRevision += revision =>
			{
				Console.WriteLine("mapping of revision {0}", revision);
			};
			dataMapper.OnTruncateRevision += revision =>
			{
				Console.WriteLine("truncating of revision {0}", revision);
			};
			dataMapper.OnCheckRevision += revision =>
			{
				Console.WriteLine("checking of revision {0}", revision);
			};
			dataMapper.OnError += message =>
			{
				Console.WriteLine(message);
			};
			return dataMapper;
		}

		static void BlameDiff(IVcsData vcsData)
		{
			var r0 = "6683463ed6b2da9eed309c305806f9393d1ae728";
			var r1 = "a4b7dbef4ef53f4fffbda0a6f5eada4c377e3fc5";
			var r2 = "b5039db6d25ae25f1cb2db541ed13602784fafc3";
			var r3 = "b51ad4314078298194d23d46e2b4473ffd32a88a";

			var file = "/rev-tree.c";

			var blame0 = vcsData.Blame(r0, file);
			var blame1 = vcsData.Blame(r1, file);
			var blame2 = vcsData.Blame(r2, file);
			var blame3 = vcsData.Blame(r3, file);
			var diff01 = blame0.Diff(blame1);
			var diff02 = blame0.Diff(blame2);
			var diff13 = blame1.Diff(blame3);
			var diff23 = blame2.Diff(blame3);

			Console.WriteLine();	
		}
	}
}
