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
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch
{
	class Program
	{
		static void Main(string[] args)
		{
			var data = new SqliteDataStore("d:/123.db");// { Logging = true };
			var gitClient = new CommandLineGitClient("D:/src/git/.git");
			var vcsData = new VcsDataCached(new GitData(gitClient), 10, 1000);
			var mapper = CreateDataMapper(data, vcsData);
			
			using (ConsoleTimeLogger.Start("time"))
			{
				Func<DataMapper.MappingSettings> settings = () => new DataMapper.MappingSettings()
				{
					StopRevision = vcsData.GetRevisionByNumber(3000),
					Check = DataMapper.CheckMode.TOUCHED,
				};
				mapper.MapRevisions(settings());
				//mapper.Truncate(2175);
				//mapper.Check(2309, DataMapper.CheckMode.ALL);

				//BlameDiff(vcsData);
				//FileHistory(data, vcsData, "/git-svnimport.perl");
				//Select(data);
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
			var r0 = "40dad96e41fcb96e31bdf11deec3c7bf6261adbe";
			var r1 = "29504118f8528f658fd0bfc02d8d78d4c01dc2cc";
			
			var file = "/git-svnimport.perl";

			var blame0 = vcsData.Blame(r0, file);
			var blame1 = vcsData.Blame(r1, file);
			var diff01 = blame0.Diff(blame1);
			
			Console.WriteLine();	
		}
		static void Select(IDataStore data)
		{
			using (var s = data.OpenSession())
			{
				int count = 0;
				using (ConsoleTimeLogger.Start("time"))
				{
					//count = s.SelectionDSL().Commits()
					//.OnBranchBack("00000000000000000101000000000100001")
					//.Count();

					count = s.SelectionDSL().Commits()
					.OnBranchForward("000000000000000001010000000001")
					.Count();
				}
				Console.WriteLine(count);
			}
		}
		static void FileHistory(IDataStore data, IVcsData vcsData, string path)
		{
			using (var s = data.OpenSession())
			{
				var modifications =
					from f in s.Get<CodeFile>().Where(x => x.Path == path)
					join m in s.Get<Modification>() on f.Id equals m.FileId
					join c in s.Get<Commit>() on m.CommitId equals c.Id
					join cb in s.Get<CodeBlock>() on m.Id equals cb.ModificationId
					group cb by new { c, m } into codeBlocksForFile
					orderby codeBlocksForFile.Key.c.OrderedNumber descending
					select new
					{
						revision = codeBlocksForFile.Key.c.Revision,
						revisionNumber = codeBlocksForFile.Key.c.OrderedNumber,
						action = codeBlocksForFile.Key.m.Action,
						codePlus = codeBlocksForFile
							.Where(x => x.TargetCodeBlockId == null)
							.Sum(x => x.Size),
						codeMinus = codeBlocksForFile
							.Where(x => x.Size < 0)
							.Sum(x => x.Size),
						cancelPlus = codeBlocksForFile
							.Where(x => x.TargetCodeBlockId != null && x.Size > 0)
							.Sum(x => x.Size),
					};

				Console.WriteLine(path);
				foreach (var m in modifications)
				{
					var lines = vcsData.Blame(m.revision, path);
					Console.WriteLine(string.Format("{0} ({1}) {2} +{3} -{4} +{5} loc: {6}",
						m.revision,
						m.revisionNumber,
						m.action,
						m.codePlus,
						-m.codeMinus,
						m.cancelPlus,
						lines.Count));
				}
			}
		}
	}
}
