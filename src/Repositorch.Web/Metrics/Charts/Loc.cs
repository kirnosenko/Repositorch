using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;

namespace Repositorch.Web.Metrics.Charts
{
	public class Loc : Metric
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
					loc = cbc.Sum()
				}).ToArray();

			var loc = codeByDate.Select(c => new
			{
				date = c.date,
				loc = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.loc)
			}).OrderBy(x => x.date).ToArray();

			return loc;
		}
	}
}
