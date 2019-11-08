using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	static class DataMapperSessionExtension
	{
		public static bool RevisionExists(this ISession s, string revision)
		{
			return s.Get<Commit>()
				.SingleOrDefault(c => c.Revision == revision) != null;
		}
		public static int MappingStartRevision(this ISession s)
		{
			return s.Get<Commit>().Count() + 1;
		}
		public static int NumberOfRevision(this ISession s, string revision)
		{
			return s.Get<Commit>()
				.Single(x => x.Revision == revision)
				.OrderedNumber;
		}
		public static string LastMappedRevision(this ISession s)
		{
			return s.Get<Commit>()
				.Single(x => x.OrderedNumber == s.Get<Commit>().Max(y => y.OrderedNumber))
				.Revision;
		}
	}

	public class DataMapper
	{
		public enum CheckMode
		{
			NOTHING,
			TOUCHED,
			ALL
		}
		public struct MappingSettings
		{
			public string StartRevision { get; set; }
			public string StopRevision { get; set; }
			public CheckMode Check { get; set; }
		}

		public event Action<string> OnMapRevision;
		public event Action<string> OnTruncateRevision;
		public event Action<string> OnCheckRevision;
		public event Action<string> OnError;

		private IDataStore data;
		private IVcsData vcsData;

		private List<Func<IRepositoryMappingExpression, IEnumerable<IRepositoryMappingExpression>>> mappers =
			new List<Func<IRepositoryMappingExpression, IEnumerable<IRepositoryMappingExpression>>>();

		public DataMapper(IDataStore data, IVcsData vcsData)
		{
			this.data = data;
			this.vcsData = vcsData;
		}
		public void RegisterMapper<IME, OME>(EntityMapper<IME, OME> mapper)
			where IME : IRepositoryMappingExpression
			where OME : IRepositoryMappingExpression
		{
			mappers.Add((exp) =>
			{
				var newExpressions = new List<IRepositoryMappingExpression>();

				if (typeof(IME).IsAssignableFrom(exp.GetType()))
				{
					foreach (var resultExp in mapper.Map((IME)exp))
					{
						newExpressions.Add(resultExp);
					}
				}

				return newExpressions;
			});
		}
		public void MapRevisions(MappingSettings settings)
		{
			int nextRevisionNumber;
			string nextRevision;

			using (var s = data.OpenSession())
			{
				if (settings.StartRevision == null)
				{
					if (s.RevisionExists(settings.StopRevision))
					{
						return;
					}
					nextRevisionNumber = s.MappingStartRevision();
					nextRevision = vcsData.GetRevisionByNumber(nextRevisionNumber);
				}
				else // partial mapping
				{
					settings.StopRevision = s.LastMappedRevision();
					nextRevision = settings.StartRevision;
					nextRevisionNumber = s.NumberOfRevision(settings.StartRevision);
				}
			}

			do
			{
				OnMapRevision?.Invoke(GetRevisionName(nextRevision, nextRevisionNumber));
				MapRevision(nextRevision);
				if (!CheckRevision(nextRevision, settings.Check, true))
				{
					Truncate(nextRevisionNumber - 1);
					break;
				}
				nextRevision = nextRevision == settings.StopRevision ?
					null
					:
					vcsData.GetRevisionByNumber(++nextRevisionNumber);
			} while (nextRevision != null);
		}
		public void MapRevision(string revision)
		{
			using (var s = data.OpenSession())
			{
				var rootExp = new RepositoryMappingExpression(s)
				{
					Revision = revision
				};
				MapData(0, rootExp);
				s.SubmitChanges();
			}
		}
		private void MapData(int mapperIndex, IRepositoryMappingExpression parentExpression)
		{
			if (mapperIndex >= mappers.Count)
			{
				return;
			}

			var mapper = mappers[mapperIndex];
			var newExpressions = mapper(parentExpression).ToArray();
			if (newExpressions.Length > 0)
			{
				foreach (var newExp in newExpressions)
				{
					MapData(mapperIndex + 1, newExp);
				}
			}
			else
			{
				MapData(mapperIndex + 1, parentExpression);
			}
		}
		public void Truncate(int revisionsToKeep)
		{
			using (var s = data.OpenSession())
			{
				var commitsToRemove = s.GetReadOnly<Commit>()
					.Skip(revisionsToKeep)
					.OrderByDescending(x => x.OrderedNumber)
					.ToArray();

				foreach (var commit in commitsToRemove)
				{
					OnTruncateRevision?.Invoke(GetRevisionName(commit.Revision, commit.OrderedNumber));

					var modificationsToRemove = s.GetReadOnly<Modification>()
						.Where(m => m.CommitId == commit.Id)
						.ToArray();
					var codeToRemove =
						(from m in modificationsToRemove
						join cb in s.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
						select cb).ToArray();
					var filesToRemove =
						(from f in s.GetReadOnly<CodeFile>()
						join m in s.GetReadOnly<Modification>()
							.Where(x => x.CommitId != commit.Id) 
							on f.Id equals m.FileId into nullm
						from nm in nullm.DefaultIfEmpty()
						where nm == null
						select f).ToArray();
					var authorsToRemove =
						(from a in s.GetReadOnly<Author>()
						join c in s.GetReadOnly<Commit>()
							.Where(x => x.Id != commit.Id)
							on a.Id equals c.AuthorId into nullc
						from nc in nullc.DefaultIfEmpty()
						where nc == null
						select a).ToArray();
					var fixesToRemove = s.GetReadOnly<BugFix>()
						.Where(x => x.CommitId == commit.Id)
						.ToArray();
					var branchesToRemove =
						(from b in s.GetReadOnly<Branch>()
						join c in s.GetReadOnly<Commit>()
							.Where(x => x.Id != commit.Id)
							on b.Id equals c.BranchId into nullc
						from nc in nullc.DefaultIfEmpty()
						where nc == null
						select b).ToArray();
					
					s.RemoveRange(codeToRemove);
					s.RemoveRange(modificationsToRemove);
					s.RemoveRange(filesToRemove);
					s.RemoveRange(fixesToRemove);
					s.Remove(commit);
					s.RemoveRange(authorsToRemove);
					s.RemoveRange(branchesToRemove);
					s.SubmitChanges();
				}
			}
		}
		public void Check(int revisionsToSkip, CheckMode mode)
		{
			var revisions = data.UsingSession(s =>
				s.Get<Commit>().Skip(revisionsToSkip).Select(c => c.Revision).ToArray());

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
					OnCheckRevision?.Invoke(GetRevisionName(revision, s.NumberOfRevision(revision)));
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
		public void CheckAndTruncate(string path)
		{
			var revisions = data.UsingSession(s => s.Get<Commit>().Count());

			while (true)
			{
				using (var s = data.OpenSession())
				{
					var revision = s.Get<Commit>()
						.OrderByDescending(x => x.OrderedNumber)
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

		private bool CheckLinesContent(ISession s, string revision, CodeFile file)
		{
			var fileBlame = vcsData.Blame(revision, file.Path);
			if (fileBlame == null)
			{
				OnError?.Invoke(string.Format("Could not get blame for file {0} in revision {1}.",
					file.Path, revision));
				return false;
			}
			
			var commitsToLookAt = s.SelectionDSL()
				.Commits().TillRevision(revision).Fixed();

			double currentLOC = commitsToLookAt
				.Files().IdIs(file.Id)
				.Modifications().InCommits().InFiles()
				.CodeBlocks().InModifications()
				.CalculateLOC();

			if (currentLOC != fileBlame.Count)
			{
				OnError?.Invoke(string.Format("Incorrect number of lines in file {0}. {1} should be {2}",
					file.Path, currentLOC, fileBlame.Count));
			}

			var linesByRevision =
			(
				from line in fileBlame
				group line by line.Value into g
				select new
				{
					Revision = g.Key,
					CodeSize = g.Count()
				}
			).ToArray();
			
			var codeBySourceRevision =
			(
				from f in s.Get<CodeFile>().Where(x => x.Id == file.Id)
				join m in s.Get<Modification>() on f.Id equals m.FileId
				join c in commitsToLookAt on m.CommitId equals c.Id
				join cb in s.Get<CodeBlock>() on m.Id equals cb.ModificationId
				join tcb in s.Get<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
				join tcbc in s.Get<Commit>() on tcb.AddedInitiallyInCommitId equals tcbc.Id
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
						? GetRevisionName(sourceCommit.Revision, sourceCommit.OrderedNumber)
						: null,
					error.CodeSize,
					error.RealCodeSize));
			}

			return currentLOC == fileBlame.Count && incorrectCode.Count == 0;
		}

		private string GetRevisionName(string revision, int number)
		{
			return string.Format("{0}{1}",
				revision,
				revision != number.ToString() ? string.Format(" ({0})", number) : "");
		}
	}
}
