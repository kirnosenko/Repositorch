using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Metrics.Charts
{
	public class Loc : IMetric
	{
		public object Calculate(IDataStore data, JObject input)
		{
			using (var s = data.OpenSession())
			{
				var commits = s.GetReadOnly<Commit>()
					.OrderBy(x => x.OrderedNumber)
					.Select(x => new
					{
						Date = x.Date,
						Number = x.OrderedNumber
					})
					.ToArray();
				var locs = commits.Select(c => new
				{
					date = c.Date,
					loc = s.SelectionDSL()
						.Commits().TillNumber(c.Number)
						.Modifications().InCommits()
						.CodeBlocks().InModifications()
						.CalculateLOC()
				}).ToArray();

				return locs;
			}
		}
	}
}
