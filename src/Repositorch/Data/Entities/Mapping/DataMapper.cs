using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data.Entities.DSL.Mapping;
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
		public event Action<string, string> OnRevisionProcessing;

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
		public void MapRevisions(string startRevision = null, string stopRevision = null)
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
					nextRevision = vcsData.RevisionByNumber(nextRevisionNumber);
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
				OnRevisionProcessing?.Invoke(nextRevision, nextRevisionNumber.ToString());
				MapRevision(nextRevision);
				nextRevision = nextRevision == stopRevision ?
					null
					:
					vcsData.RevisionByNumber(++nextRevisionNumber);
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
					OnRevisionProcessing?.Invoke(c.Revision, c.OrderedNumber.ToString());

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
	}
}
