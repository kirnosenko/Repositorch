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
				//mapper.Truncate(125);
				//mapper.Check(120, DataMapper.CheckMode.ALL);
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
	}
}
