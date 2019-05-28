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
using Repositorch.Data.Entities.DSL.Selection.Metrics;

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
			//Mapping(data, vcsData, 200);
			//Truncate(data, vcsData, 124);
			Check(data, vcsData, 100);
			Console.ReadKey();
		}
		static void Selection(IDataStore data)
		{
			using (ConsoleTimeLogger.Start("selection time"))
			{
			
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
				new BugFixMapper(vcsData, new BugFixDetectorBasedOnLogMessage()));
			dataMapper.RegisterMapper(
				new CodeFileMapper(vcsData));
			dataMapper.RegisterMapper(
				new ModificationMapper(vcsData));
			dataMapper.RegisterMapper(
				new CodeBlockMapper(vcsData));
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

				mapper.MapRevisions(stopRevision: vcsData.RevisionByNumber(revisions));
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
		public static void Check(IDataStore data, IVcsData vcsData, int skipRevisions = 0)
		{
			using (ConsoleTimeLogger.Start("checking time"))
			using (var s = data.OpenSession())
			{
				int counter = 1 + skipRevisions;
				foreach (var revision in s.Get<Commit>().Skip(skipRevisions).Select(c => c.Revision))
				{
					Console.WriteLine("checking of revision ({0}) {1}", revision, counter++);

					var touchedFiles = s.SelectionDSL()
						.Commits().RevisionIs(revision)
						.Files().TouchedInCommits().ExistInRevision(revision);

					foreach (var touchedFile in touchedFiles)
					{
						CheckLinesContent(s, vcsData, revision, touchedFile);
					}
				}
			}
		}
		private static void CheckLinesContent(ISession s, IVcsData vcsData, string revision, CodeFile file)
		{
			IBlame fileBlame = null;
			try
			{
				fileBlame = vcsData.Blame(revision, file.Path);
			}
			catch
			{
			}
			if (fileBlame == null)
			{
				Console.WriteLine("Could not get blame for file {0} in revision {1}.",
					file.Path, revision);
				return;
			}

			double currentLOC = s.SelectionDSL()
					.Commits().TillRevision(revision)
					.Files().IdIs(file.Id)
					.Modifications().InCommits().InFiles()
					.CodeBlocks().InModifications()
					.CalculateLOC();

			bool correct = currentLOC == fileBlame.Count;

			if (!correct)
			{
				Console.WriteLine("Incorrect number of lines in file {0}. {1} should be {2}",
					file.Path, currentLOC, fileBlame.Count);
				return;
			}

			SmartDictionary<string, int> linesByRevision = new SmartDictionary<string, int>(x => 0);
			foreach (var line in fileBlame)
			{
				linesByRevision[line.Value]++;
			}

			var codeBySourceRevision =
			(
				from f in s.Get<CodeFile>()
				join m in s.Get<Modification>() on f.Id equals m.FileId
				join cb in s.Get<CodeBlock>() on m.Id equals cb.ModificationId
				join c in s.Get<Commit>() on m.CommitId equals c.Id
				let addedCodeBlock = s.Get<CodeBlock>()
					.Single(x => x.Id == (cb.Size < 0 ? cb.TargetCodeBlockId : cb.Id))
				let codeAddedInitiallyInRevision = s.Get<Commit>()
					.Single(x => x.Id == addedCodeBlock.AddedInitiallyInCommitId)
					.Revision
				let testRevisionNumber = s.Get<Commit>()
					.Single(x => x.Revision == revision)
					.OrderedNumber
				where
					f.Id == file.Id
					&&
					c.OrderedNumber <= testRevisionNumber
				group cb.Size by codeAddedInitiallyInRevision into g
				select new
				{
					FromRevision = g.Key,
					CodeSize = g.Sum()
				}
			).Where(x => x.CodeSize != 0).ToList();

			var errorCode =
				(
					from codeFromRevision in codeBySourceRevision
					where
						codeFromRevision.CodeSize != linesByRevision[codeFromRevision.FromRevision]
					select new
					{
						SourceRevision = codeFromRevision.FromRevision,
						CodeSize = codeFromRevision.CodeSize,
						RealCodeSize = linesByRevision[codeFromRevision.FromRevision]
					}
				).ToList();

			correct =
				correct
				&&
				codeBySourceRevision.Count() == linesByRevision.Count
				&&
				errorCode.Count == 0;

			if (codeBySourceRevision.Count() != linesByRevision.Count)
			{
				Console.WriteLine("Number of revisions file {0} contains code from is incorrect. {1} should be {2}",
					file.Path, codeBySourceRevision.Count(), linesByRevision.Count
				);
			}
			foreach (var error in errorCode)
			{
				Console.WriteLine("Incorrect number of lines in file {0} from revision {1}. {2} should be {3}",
					file.Path,
					error.SourceRevision,
					error.CodeSize,
					error.RealCodeSize
				);
			}	
		}
	}
}
