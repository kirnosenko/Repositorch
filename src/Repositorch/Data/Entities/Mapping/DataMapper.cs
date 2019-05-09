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
			return s.Get<Commit>().SingleOrDefault(c => c.Revision == revision) != null;
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
		public event Action<string, string> OnRevisionMapping;

		private IVcsData vcsData;

		private List<object> availableExpressions;
		private Dictionary<Type, Action> mappers = new Dictionary<Type, Action>();

		public DataMapper(IVcsData vcsData)
		{
			this.vcsData = vcsData;
			CreateDataBase = false;
		}
		public void Map(IDataStore data)
		{
			int nextRevisionNumber;
			string nextRevision;

			using (var s = data.OpenSession(true))
			{
				if (StartRevision == null)
				{
					if (CreateDataBase)
					{
						//CreateSchema(data);
					}
					else if (s.RevisionExists(StopRevision))
					{
						return;
					}
					nextRevisionNumber = s.MappingStartRevision();
					nextRevision = vcsData.RevisionByNumber(nextRevisionNumber);
				}
				else
				{
					StopRevision = s.LastMappedRevision();
					nextRevision = StartRevision;
					nextRevisionNumber = s.NumberOfRevision(StartRevision);
				}
			}

			do
			{
				if (OnRevisionMapping != null)
				{
					OnRevisionMapping(nextRevision, nextRevisionNumber.ToString());
				}
				Map(data, nextRevision);
				nextRevision = nextRevision == StopRevision ?
					null
					:
					vcsData.RevisionByNumber(++nextRevisionNumber);
			} while (nextRevision != null);
		}
		public void Map(IDataStore data, string revision)
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
		public void KeepOnlyMappers(Type[] mapperTypesToKeep)
		{
			var mappersToRemove = mappers.Where(x => !mapperTypesToKeep.Contains(x.Key)).ToList();
			foreach (var mapper in mappersToRemove)
			{
				mappers.Remove(mapper.Key);
			}
		}
		public IMapper[] Mappers
		{
			set
			{
				foreach (var mapper in value)
				{
					mapper.RegisterHost(this);
				}
			}
		}
		public bool CreateDataBase
		{
			get; set;
		}
		public string StartRevision
		{
			get; set;
		}
		public string StopRevision
		{
			get; set;
		}
	}
}
