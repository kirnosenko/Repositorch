using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;

namespace Repositorch.Web.Metrics.Charts.LOC
{
	public class Remains : Metric
	{
		protected override object Calculate(ISession s, JObject input)
		{
			var yearMin = s.Get<Commit>().Min(x => x.Date).Year;
			var yearMax = s.Get<Commit>().Max(x => x.Date).Year;
			var years = Enumerable.Range(yearMin, yearMax - yearMin + 1).ToArray();

			var remainingСodeByDate = years.Select(year => new
			{
				year = year,
				code = (
					from c in s.GetReadOnly<Commit>()
					join m in s.GetReadOnly<Modification>() on c.Id equals m.CommitId
					join cb in s.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
					join tcb in s.GetReadOnly<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
					join tcbc in s.GetReadOnly<Commit>() on tcb.AddedInitiallyInCommitId equals tcbc.Id
					where tcbc.Date.Year == year
					group cb.Size by c.Date into cbc
					select new
					{
						date = cbc.Key,
						locTotal = cbc.Sum()
					}).ToArray()
			}).ToArray();

			var dates = remainingСodeByDate
				.SelectMany(x => x.code.Select(c => c.date))
				.Distinct()
				.OrderBy(x => x)
				.ToArray();

			var loc = dates
				.Select(date =>
				{
					var data = new Dictionary<string, object>()
					{
						{ "date", date },
					};
					foreach (var year in years)
					{
						var codeSize = remainingСodeByDate
							.Single(x => x.year == year).code
							.Where(x => x.date <= date)
							.Sum(x => x.locTotal);
						if (codeSize > 0)
						{
							data.Add(year.ToString(), codeSize);
						}
					}
					return data;
				}).ToArray();

			return new
			{
				keys = years.Select(x => x.ToString()).ToArray(),
				values = loc,
			};
		}
	}
}
