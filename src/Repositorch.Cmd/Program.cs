using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.Mapping;
using Repositorch.Data.VersionControl;
using Repositorch.Data.VersionControl.Git;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.Persistent;

namespace Repositorch
{
	class Program
	{
		static void Main(string[] args)
		{
			var data = new SqlServerDataStore("git")
			//var data = new PostgreSqlDataStore("git", "postgres", "123")
			{
				//Logging = true,
				//SingletonSession = true,
			};
			var gitClient = new CommandLineGitClient("D:/src/git/.git");
			var vcsData = new VcsDataCached(gitClient, 1000, 1000);
			var mapper = CreateDataMapper(data, vcsData);
			
			using (ConsoleTimeLogger.Start("time"))
			{
				Func<DataMapper.MappingSettings> settings = () => new DataMapper.MappingSettings()
				{
					StopRevision = vcsData.GetRevisionByNumber(3000),
					Check = DataMapper.CheckMode.TOUCHED,
				};
				mapper.MapRevisions(settings());
				//mapper.Truncate(1170);
				//mapper.Check(2309, DataMapper.CheckMode.ALL);
				//mapper.CheckAndTruncate("/test-delta.c");

				//BlameDiff(vcsData);
				//FileHistory(data, vcsData, "/Documentation/merge-pull-opts.txt");
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
				new BlamePreLoader(vcsData));
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
			var r0 = "41f93a2c903a45167b26c2dc93d45ffa9a9bbd49";
			var r1 = "c2f3bf071ee90b01f2d629921bb04c4f798f02fa";
			
			var file = "/test-delta.c";

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
					(from f in s.GetReadOnly<CodeFile>()
					join m in s.GetReadOnly<Modification>() on f.Id equals m.FileId
					join c in s.GetReadOnly<Commit>() on m.CommitId equals c.Id
					join cb in s.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
					where f.Path == path
					group cb by new { m.Action, c.Revision, c.OrderedNumber, c.Date } into modCode
					select new
					{
						Action = modCode.Key.Action,
						Revision = modCode.Key.Revision,
						Number = modCode.Key.OrderedNumber,
						Date = modCode.Key.Date,
						Added = modCode.Sum(
							x => x.TargetCodeBlockId == null ? x.Size : 0),
						Removed = modCode.Sum(
							x => x.Size < 0 ? x.Size : 0),
						Compensated = modCode.Sum(
							x => (x.TargetCodeBlockId != null && x.Size > 0) ? x.Size : 0),
					}).ToArray();

				Console.WriteLine(path);
				
				foreach (var m in modifications.OrderByDescending(x => x.Number))
				{
					var linesCount = (m.Action == TouchedFileAction.REMOVED)
						? 0
						: vcsData.Blame(m.Revision, path).Count();
					Console.WriteLine(string.Format("{0} ({1}) {2} {3} +{4} -{5} +{6} loc: {7}",
						m.Revision,
						m.Number,
						m.Date.ToShortDateString(),
						m.Action,
						m.Added,
						Math.Abs(m.Removed),
						m.Compensated,
						linesCount));
				}
			}
		}
	}
}
