using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;

namespace Repositorch.Web.Metrics.Charts.LOC
{
	public class Summary : Metric
	{
		private class Settings
		{
			public bool LocTotal { get; set; }
			public bool LocAdded { get; set; }
			public bool LocRemoved { get; set; }
			public string Author { get; set; }
			public string Path { get; set; }
		}

		public override object GetSettings(IRepository repository)
		{
			return new
			{
				settings = new Settings()
				{
					LocTotal = true,
					LocAdded = false,
					LocRemoved = false,
					Author = null,
					Path = null,
				},
				authors = repository.GetReadOnly<Author>()
					.Select(x => x.Name)
					.ToArray(),
			};
		}
		public override object Calculate(IRepository repository, JObject input)
		{
			var settings = input.ToObject<Settings>();

			var codeByDate = (
				from c in repository.GetReadOnly<Commit>()
				join m in repository.GetReadOnly<Modification>() on c.Id equals m.CommitId
				join cb in repository.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
				group cb.Size by c.Date into cbc
				select new
				{
					date = cbc.Key,
					locTotal = cbc.Sum(),
					locAdded = cbc.Sum(x => x > 0 ? x : 0),
					locRemoved = cbc.Sum(x => x < 0 ? -x : 0),
				}).ToArray();

			var loc = codeByDate.Select(c => new
			{
				date = c.date,
				locTotal = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.locTotal),
				locAdded = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.locAdded),
				locRemoved = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.locRemoved),
			}).OrderBy(x => x.date).ToArray();

			return loc;
		}
	}
}
