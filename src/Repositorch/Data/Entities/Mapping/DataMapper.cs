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

	public class DataMapper : IMappingHost
	{
		public event Action<string> OnMapRevision;
		public event Action<string> OnTruncateRevision;
		public event Action<string> OnCheckRevision;
		public event Action<string> OnError;

		private IDataStore data;
		private IVcsData vcsData;

		private List<object> availableExpressions;
		private Dictionary<Type, Action> mappers = new Dictionary<Type, Action>();

		public DataMapper(IDataStore data, IVcsData vcsData)
		{
			this.data = data;
			this.vcsData = vcsData;
		}
		public void RegisterMapper<T, IME, OME>(EntityMapper<T, IME, OME> mapper)
		{
			if (mappers.ContainsKey(typeof(T)))
			{
				mappers.Remove(typeof(T));
			}
			mappers.Add(typeof(T), () =>
			{
				List<object> newExpressions = new List<object>();

				foreach (var iExp in availableExpressions.Where(x => x.GetType() == typeof(IME)))
				{
					foreach (var oExp in mapper.Map((IME)iExp))
					{
						newExpressions.Add(oExp);
					}
				}

				foreach (var exp in newExpressions)
				{
					availableExpressions.Add(exp);
				}
			});
		}
		public void MapRevisions(string startRevision = null, string stopRevision = null, bool check = false)
		{
			int nextRevisionNumber;
			string nextRevision;

			using (var s = data.OpenSession())
			{
				if (startRevision == null)
				{
					if (s.RevisionExists(stopRevision))
					{
						return;
					}
					nextRevisionNumber = s.MappingStartRevision();
					nextRevision = vcsData.GetRevisionByNumber(nextRevisionNumber);
				}
				else
				{
					stopRevision = s.LastMappedRevision();
					nextRevision = startRevision;
					nextRevisionNumber = s.NumberOfRevision(startRevision);
				}
			}

			do
			{
				OnMapRevision?.Invoke(GetRevisionName(nextRevision, nextRevisionNumber));
				MapRevision(nextRevision);
				if (check)
				{
					if (!CheckRevision(nextRevision, true, true))
					{
						break;
					}
				}
				nextRevision = nextRevision == stopRevision ?
					null
					:
					vcsData.GetRevisionByNumber(++nextRevisionNumber);
			} while (nextRevision != null);
		}
		public void MapRevision(string revision)
		{
			using (var s = data.OpenSession())
			{
				availableExpressions = new List<object>()
				{ 
					new RepositoryMappingExpression(s)
					{
						Revision = revision
					}
				};

				foreach (var mapper in mappers)
				{
					mapper.Value();
				}

				s.SubmitChanges();
			}
		}
		public void Truncate(int revisionsToKeep)
		{
			using (var s = data.OpenSession())
			{
				var commitsToRemove = s.Get<Commit>()
					.Skip(revisionsToKeep)
					.OrderByDescending(x => x.OrderedNumber)
					.ToArray();
				
				foreach (var c in commitsToRemove)
				{
					OnTruncateRevision?.Invoke(GetRevisionName(c.Revision, c.OrderedNumber));

					var modificationsToRemove = s.Get<Modification>()
						.Where(m => m.CommitId == c.Id);
					var codeToRemove =
						from m in modificationsToRemove
						join cb in s.Get<CodeBlock>() on m.Id equals cb.ModificationId
						select cb;
					s.RemoveRange(codeToRemove);
					s.RemoveRange(modificationsToRemove);
					var filesToRemove = s.Get<CodeFile>()
						.Where(x => x.AddedInCommitId == c.Id);
					s.RemoveRange(filesToRemove);
					foreach (var f in s.Get<CodeFile>().Where(x => x.DeletedInCommitId == c.Id))
					{
						f.DeletedInCommit = null;
					}
					var fixToRemove = s.Get<BugFix>().SingleOrDefault(x => x.CommitId == c.Id);
					if (fixToRemove != null)
					{
						s.Remove(fixToRemove);
					}
					s.Remove(c);
					
					s.SubmitChanges();
				}
			}
		}
		public bool CheckRevision(string revision, bool touchedOnly = true, bool mute = false)
		{
			bool result = true;

			using (var s = data.OpenSession())
			{
				if (!mute)
				{
					OnCheckRevision?.Invoke(GetRevisionName(revision, s.NumberOfRevision(revision)));
				}
				var touchedFiles = s.SelectionDSL()
					.Commits().RevisionIs(revision)
					.Files().Reselect(
						e => touchedOnly ? e.TouchedInCommits() : e)
					.ExistInRevision(revision);

				foreach (var touchedFile in touchedFiles)
				{
					result &= CheckLinesContent(s, revision, touchedFile);
				}
			}

			return result;
		}
		private bool CheckLinesContent(ISession s, string revision, CodeFile file)
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
				OnError?.Invoke(string.Format("Could not get blame for file {0} in revision {1}.",
					file.Path, revision));
				return false;
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
				OnError?.Invoke(string.Format("Incorrect number of lines in file {0}. {1} should be {2}",
					file.Path, currentLOC, fileBlame.Count));
				return false;
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
				OnError?.Invoke(string.Format("Number of revisions file {0} contains code from is incorrect. {1} should be {2}",
					file.Path, codeBySourceRevision.Count(), linesByRevision.Count));
			}
			foreach (var error in errorCode)
			{
				OnError?.Invoke(string.Format("Incorrect number of lines in file {0} from revision {1}. {2} should be {3}",
					file.Path,
					error.SourceRevision,
					error.CodeSize,
					error.RealCodeSize));
			}

			return correct;
		}

		private string GetRevisionName(string revision, int number)
		{
			return string.Format("{0}{1}",
				revision,
				revision != number.ToString() ? string.Format(" ({0})", number) : "");
		}
	}
}
