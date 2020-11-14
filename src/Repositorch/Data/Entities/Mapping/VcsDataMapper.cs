﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public class VcsDataMapper
	{
		public enum MappingResult
		{
			STOPPED,
			SUCCESS,
			ERROR
		}
		public enum CheckMode
		{
			NOTHING,
			TOUCHED,
			ALL
		}
		public class MappingSettings
		{
			/// <summary>
			/// Number of revisions to limit mapping.
			/// </summary>
			public int? RevisionLimit { get; set; }
			public bool FastMergeProcessing { get; set; }
			public string FixMessageKeyWords { get; set; }
			public string FixMessageStopWords { get; set; }
			/// <summary>
			/// Mode to check files after mapping.
			/// </summary>
			public CheckMode CheckMode { get; set; }
		}

		public event Action<string> OnMapRevision;
		public event Action<string> OnTruncateRevision;
		public event Action<string> OnCheckRevision;
		public event Action<string> OnError;

		private readonly IDataStore data;
		private readonly IVcsData vcsData;
		private readonly MappingSettings settings;

		private readonly List<Func<IEnumerable<IRepositoryMappingExpression>, IEnumerable<IRepositoryMappingExpression>>> mappers =
			new List<Func<IEnumerable<IRepositoryMappingExpression>, IEnumerable<IRepositoryMappingExpression>>>();

		public VcsDataMapper(
			IDataStore data,
			IVcsData vcsData,
			MappingSettings settings)
		{
			this.data = data;
			this.vcsData = vcsData;
			this.settings = settings;
		}
		public void RegisterMapper<IME, OME>(Mapper<IME, OME> mapper, bool parallel = false)
			where IME : class, IRepositoryMappingExpression
			where OME : class, IRepositoryMappingExpression
		{
			mappers.Add((expressions) =>
			{
				if (!parallel)
				{
					var newExpressions = new List<IRepositoryMappingExpression>();
					foreach (var exp in expressions)
					{
						IME inputExp = exp as IME;
						if (inputExp != null)
						{
							foreach (var resultExp in mapper.Map(inputExp))
							{
								newExpressions.Add(resultExp);
							}
						}
					}
					return newExpressions;
				}
				else
				{
					var newExpressions = new ConcurrentBag<IRepositoryMappingExpression>();
					Parallel.ForEach(
						expressions,
						new ParallelOptions
						{
							MaxDegreeOfParallelism = Math.Min(expressions.Count(), 16)
						},
						exp =>
						{
							IME inputExp = exp as IME;
							if (inputExp != null)
							{
								foreach (var resultExp in mapper.Map(inputExp))
								{
									newExpressions.Add(resultExp);
								}
							}
						});
					return newExpressions;
				}
			});
		}
		public MappingResult MapRevisions(CancellationToken stopToken = default)
		{
			int nextRevisionNumber = data.UsingSession(s =>
				s.Get<Commit>().Count() + 1);
			string nextRevision = vcsData.GetRevisionByNumber(nextRevisionNumber);

			while (nextRevision != null)
			{
				if (stopToken.IsCancellationRequested)
				{
					return MappingResult.STOPPED;
				}
				OnMapRevision?.Invoke(GetRevisionName(nextRevision, nextRevisionNumber));
				MapRevision(nextRevision);
				if (!CheckRevision(nextRevision, settings.CheckMode, true))
				{
					Truncate(nextRevisionNumber - 1);
					return MappingResult.ERROR;
				}
				nextRevision = ++nextRevisionNumber > (settings.RevisionLimit ?? int.MaxValue) ?
					null
					:
					vcsData.GetRevisionByNumber(nextRevisionNumber);
			}

			return MappingResult.SUCCESS;
		}
		public void MapRevision(string revision)
		{
			using (var s = data.OpenSession())
			{
				IEnumerable<IRepositoryMappingExpression> expressions =
					new IRepositoryMappingExpression[]
					{
						new RepositoryMappingExpression(s)
						{
							Revision = revision
						}
					};
				foreach (var mapper in mappers)
				{
					var newExpressions = mapper(expressions);
					if (newExpressions.Count() > 0)
					{
						expressions = newExpressions;
					}
				}
				s.SubmitChanges();
			}
		}
		public void Truncate(int revisionsToKeep)
		{
			using (var s = data.OpenSession())
			{
				var commitsToRemove = s.GetReadOnly<Commit>()
					.Skip(revisionsToKeep)
					.OrderByDescending(x => x.Number)
					.ToArray();

				foreach (var commit in commitsToRemove)
				{
					OnTruncateRevision?.Invoke(GetRevisionName(commit.Revision, commit.Number));

					var modificationsToRemove = s.GetReadOnly<Modification>()
						.Where(m => m.CommitNumber == commit.Number)
						.ToArray();
					var codeToRemove =
						(from m in modificationsToRemove
						join cb in s.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
						select cb).ToArray();
					var filesToRemove =
						(from f in s.GetReadOnly<CodeFile>()
						join m in s.GetReadOnly<Modification>()
							.Where(x => x.CommitNumber != commit.Number)
							on f.Id equals m.FileId into nullm
						from nm in nullm.DefaultIfEmpty()
						where nm == null
						select f).ToArray();
					var authorsToRemove =
						(from a in s.GetReadOnly<Author>()
						join c in s.GetReadOnly<Commit>()
							.Where(x => x.Number != commit.Number)
							on a.Id equals c.AuthorId into nullc
						from nc in nullc.DefaultIfEmpty()
						where nc == null
						select a).ToArray();
					var attributesToRemove = s.GetReadOnly<CommitAttribute>()
						.Where(x => x.CommitNumber == commit.Number)
						.ToArray();
					var branchesToRemove =
						(from b in s.GetReadOnly<Branch>()
						join c in s.GetReadOnly<Commit>()
							.Where(x => x.Number != commit.Number)
							on b.Id equals c.BranchId into nullc
						from nc in nullc.DefaultIfEmpty()
						where nc == null
						select b).ToArray();
					
					s.RemoveRange(codeToRemove);
					s.RemoveRange(modificationsToRemove);
					s.RemoveRange(filesToRemove);
					s.RemoveRange(attributesToRemove);
					s.Remove(commit);
					s.RemoveRange(authorsToRemove);
					s.RemoveRange(branchesToRemove);
					s.SubmitChanges();
				}
			}
		}
		public void TruncateToContinue()
		{
			var firstLostSplit = GetFirstLostSplit();
			if (firstLostSplit != null)
			{
				Truncate(firstLostSplit.Number - 1);
			}
		}
		public Commit GetFirstLostSplit()
		{
			using (var s = data.OpenSession())
			{
				var lastCommit = s.Get<Commit>()
					.OrderByDescending(x => x.Number)
					.FirstOrDefault();
				if (lastCommit == null)
				{
					return null;
				}

				var splitRevisions = vcsData
					.GetSplitRevisionsTillRevision(lastCommit.Revision)
					.ToArray();
				var mappedSplitCount = s.SelectionDSL()
					.Commits().AreSplits().Count();
				if (splitRevisions.Length == mappedSplitCount)
				{
					return null;
				}
				var mappedSplitRevisions = s.SelectionDSL()
					.Commits().AreSplits()
					.Select(x => x.Revision)
					.ToArray();
				var lostSplitRevisions = splitRevisions
					.Except(mappedSplitRevisions)
					.ToArray();
				var firstLostSplit = s.SelectionDSL()
					.Commits().RevisionIsIn(lostSplitRevisions)
					.OrderBy(x => x.Number)
					.FirstOrDefault();

				return firstLostSplit;
			}
		}
		public void Check(int revisionsToSkip, CheckMode mode)
		{
			var revisions = data.UsingSession(s => s.Get<Commit>()
				.OrderBy(x => x.Number)
				.Skip(revisionsToSkip)
				.Select(c => c.Revision)
				.ToArray());

			foreach (var r in revisions)
			{
				if (!CheckRevision(r, mode))
				{
					return;
				}
			}
		}
		public bool CheckRevision(string revision, CheckMode mode, bool mute = false)
		{
			if (mode == CheckMode.NOTHING)
			{
				return true;
			}
			
			using (var s = data.OpenSession())
			{
				if (!mute)
				{
					var revisionNumber = s.Get<Commit>()
						.Single(x => x.Revision == revision)
						.Number;
					OnCheckRevision?.Invoke(GetRevisionName(revision, revisionNumber));
				}
				var filesToCheck = s.SelectionDSL()
					.Commits().RevisionIs(revision)
					.Files().Reselect(
						e => mode == CheckMode.TOUCHED ? e.TouchedInCommits() : e)
					.ExistInRevision(revision)
					.ToArray();

				var result = true;
				foreach (var file in filesToCheck)
				{
					result &= CheckLinesContent(s, revision, file);
				}
				return result;
			}
		}
		public bool CheckFile(string revision, string path)
		{
			using (var s = data.OpenSession())
			{
				var file = s.Get<CodeFile>().SingleOrDefault(x => x.Path == path);
				return CheckLinesContent(s, revision, file);
			}
		}
		public void CheckAndTruncate(string path)
		{
			var revisions = data.UsingSession(s => s.Get<Commit>().Count());

			while (true)
			{
				using (var s = data.OpenSession())
				{
					var revision = s.Get<Commit>()
						.OrderByDescending(x => x.Number)
						.Select(x => x.Revision)
						.FirstOrDefault();
					var file = s.Get<CodeFile>()
						.Where(x => x.Path == path)
						.OrderByDescending(x => x.Id)
						.FirstOrDefault();
					if (revision == null || file == null)
					{
						return;
					}
					var checkSuccess = CheckLinesContent(s, revision, file);
					if (checkSuccess)
					{
						return;
					}
				}

				revisions--;
				Truncate(revisions);
			}
		}
		public static VcsDataMapper ConstructDataMapper(
			IDataStore data,
			IVcsData vcsData,
			MappingSettings settings)
		{
			VcsDataMapper dataMapper = new VcsDataMapper(data, vcsData, settings);
			dataMapper.RegisterMapper(new CommitMapper(vcsData));
			dataMapper.RegisterMapper(new TagMapper(vcsData));
			var bugFixDetector = new BugFixDetectorBasedOnLogMessage();
			if (settings.FixMessageKeyWords != null)
			{
				bugFixDetector.KeyWords = settings.FixMessageKeyWords.Split(' ');
			}
			if (settings.FixMessageStopWords != null)
			{
				bugFixDetector.StopWords = settings.FixMessageStopWords.Split(' ');
			}
			dataMapper.RegisterMapper(new BugFixMapper(vcsData, bugFixDetector));
			dataMapper.RegisterMapper(new CommitAttributeMapper(vcsData));
			dataMapper.RegisterMapper(new AuthorMapper(vcsData));
			dataMapper.RegisterMapper(new BranchMapper(vcsData));
			dataMapper.RegisterMapper(new CodeFileMapper(vcsData)
			{
				FastMergeProcessing = settings.FastMergeProcessing
			});
			dataMapper.RegisterMapper(new ModificationMapper(vcsData));
			dataMapper.RegisterMapper(new BlamePreLoader(vcsData), true);
			dataMapper.RegisterMapper(new CodeBlockMapper(vcsData));

			return dataMapper;
		}

		private bool CheckLinesContent(ISession s, string revision, CodeFile file)
		{
			var fileBlame = vcsData.Blame(revision, file.Path);
			var commitsToLookAt = s.SelectionDSL()
				.Commits().TillRevision(revision).Fixed();
			double currentLOC = commitsToLookAt
				.Files().IdIs(file.Id)
				.Modifications().InCommits().InFiles()
				.CodeBlocks().InModifications()
				.CalculateLOC();
			
			if (fileBlame == null)
			{
				if (currentLOC == 0)
				{
					return true;
				}
				else
				{
					OnError?.Invoke(string.Format("Could not get blame for file {0} in revision {1}.",
						file.Path, revision));
					return false;
				}
			}

			if (currentLOC != fileBlame.Values.Sum())
			{
				OnError?.Invoke(string.Format("Incorrect number of lines in file {0}. {1} should be {2}",
					file.Path, currentLOC, fileBlame.Values.Sum()));
			}

			var linesByRevision = fileBlame.Select(x => new
			{
				Revision = x.Key,
				CodeSize = x.Value
			}).ToArray();
			
			var codeBySourceRevision =
			(
				from f in s.Get<CodeFile>().Where(x => x.Id == file.Id)
				join m in s.Get<Modification>() on f.Id equals m.FileId
				join c in commitsToLookAt on m.CommitNumber equals c.Number
				join cb in s.Get<CodeBlock>() on m.Id equals cb.ModificationId
				join tcb in s.Get<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
				join tcbc in s.Get<Commit>() on tcb.AddedInitiallyInCommitNumber equals tcbc.Number
				group cb.Size by tcbc.Revision into g
				select new
				{
					Revision = g.Key,
					CodeSize = g.Sum()
				}
			).Where(x => x.CodeSize != 0).ToArray();

			var fileRevisions = linesByRevision.Select(x => x.Revision)
				.Union(codeBySourceRevision.Select(x => x.Revision)).ToArray();

			var incorrectCode =
			(
				from fileRevision in fileRevisions
				join codeFromRevision in codeBySourceRevision on fileRevision equals codeFromRevision.Revision into cg
				from codeFromRevisionNullable in cg.DefaultIfEmpty()
				join linesFromRevision in linesByRevision on fileRevision equals linesFromRevision.Revision into lg
				from linesFromRevisionNullable in lg.DefaultIfEmpty()
				let code = codeFromRevisionNullable != null ? codeFromRevisionNullable.CodeSize : 0
				let lines = linesFromRevisionNullable != null ? linesFromRevisionNullable.CodeSize : 0
				where code != lines
				select new
				{
					SourceRevision = fileRevision,
					CodeSize = code,
					RealCodeSize = lines
				}
			).ToList();

			foreach (var error in incorrectCode)
			{
				var sourceCommit = s.Get<Commit>()
					.Where(x => x.Revision == error.SourceRevision)
					.SingleOrDefault();
				OnError?.Invoke(string.Format("Incorrect number of lines in file {0} from revision {1}. {2} should be {3}",
					file.Path,
					sourceCommit != null
						? GetRevisionName(sourceCommit.Revision, sourceCommit.Number)
						: null,
					error.CodeSize,
					error.RealCodeSize));
			}

			return currentLOC == fileBlame.Values.Sum() && incorrectCode.Count == 0;
		}

		private string GetRevisionName(string revision, int number)
		{
			return string.Format("{0}{1}",
				revision,
				revision != number.ToString() ? string.Format(" ({0})", number) : "");
		}
	}
}
