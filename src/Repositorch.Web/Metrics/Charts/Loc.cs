using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Metrics.Charts
{
	public class Loc : Metric
	{
		protected override object Calculate(ISession s, JObject input)
		{
			var commits = s.GetReadOnly<Commit>()
				.OrderBy(x => x.OrderedNumber)
				.Select(x => new
				{
					Date = x.Date,
					Number = x.OrderedNumber
				})
				.ToArray();
			var loc = commits.Select(c => new
			{
				date = c.Date,
				loc = s.SelectionDSL()
					.Commits().TillNumber(c.Number)
					.Modifications().InCommits()
					.CodeBlocks().InModifications()
					.CalculateLOC()
			}).ToArray();

			return loc;
		}
	}
}
