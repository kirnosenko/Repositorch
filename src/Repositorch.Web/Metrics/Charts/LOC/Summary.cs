using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;

namespace Repositorch.Web.Metrics.Charts.LOC
{
	public class Summary : Metric
	{
		protected override object Calculate(ISession s, JObject input)
		{
			var codeByDate = (
				from c in s.GetReadOnly<Commit>()
				join m in s.GetReadOnly<Modification>() on c.Id equals m.CommitId
				join cb in s.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
				group cb.Size by c.Date into cbc
				select new
				{
					date = cbc.Key,
					loc_total = cbc.Sum(),
					loc_added = cbc.Sum(x => x > 0 ? x : 0),
					loc_removed = cbc.Sum(x => x < 0 ? -x : 0),
				}).ToArray();

			var loc = codeByDate.Select(c => new
			{
				date = c.date,
				locTotal = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.loc_total),
				locAdded = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.loc_added),
				locRemoved = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.loc_removed),
			}).OrderBy(x => x.date).ToArray();

			return loc;
		}
	}
}
