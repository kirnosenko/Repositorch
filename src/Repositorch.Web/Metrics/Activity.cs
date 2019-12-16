using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Metrics
{
	public class Activity : IMetric
	{
		public object Calculate(IDataStore data, JObject input)
		{
			using (var s = data.OpenSession())
			{
				double totalLoc = s.SelectionDSL()
					.CodeBlocks().CalculateLOC();

				var periodFrames = SplitPeriod(
					s.GetReadOnly<Commit>().Min(x => x.Date),
					s.GetReadOnly<Commit>().Max(x => x.Date));

				var periods =
					(from frame in periodFrames
					 let commits = s.SelectionDSL().Commits()
						 .FromDate(frame.start)
						 .BeforeDate(frame.end)
						 .Fixed()
					 let code = commits
						 .Modifications().InCommits()
						 .CodeBlocks().InModifications().Fixed()
					 let totalCommits = s.SelectionDSL().Commits()
						 .BeforeDate(frame.end)
						 .Fixed()
					 let totalCode = totalCommits
						 .Modifications().InCommits()
						 .CodeBlocks().InModifications().Fixed()
					 let commitsCount = commits.Count()
					 let totalCommitsCount = totalCommits.Count()
					 let authorsCount = commits.
						 Authors().OfCommits().Count()
					 let totalAuthorsCount = totalCommits
						 .Authors().OfCommits().Count()
					 let lastRevision = commits
						 .SingleOrDefault(c => c.OrderedNumber == commits.Max(x => x.OrderedNumber))?.Revision
					 select new
					 {
						 title = string.Format("{0}-{1:00}", frame.start.Year, frame.start.Month),
						 commits = string.Format("{0} ({1})",
							 commitsCount,
							 totalCommitsCount
						 ),
						 authors = string.Format("{0} ({1})",
							 authorsCount,
							 totalAuthorsCount
						 ),
						 files = lastRevision == null ? 0 : s.SelectionDSL()
							 .Files().ExistInRevision(lastRevision).Count(),
						 locAdded = string.Format("{0} ({1})",
							 code.Added().CalculateLOC(),
							 totalCode.Added().CalculateLOC()
						 ),
						 locRemoved = string.Format("{0} ({1})",
							 -code.Removed().CalculateLOC(),
							 -totalCode.Removed().CalculateLOC()
						 ),
						 loc = totalCode.CalculateLOC()
					 }).OrderBy(x => x.title).ToArray();

				return new { periods };
			}
		}

		private IEnumerable<(DateTime start, DateTime end)> SplitPeriod(DateTime from, DateTime to)
		{
			DateTime start = new DateTime(from.Year, from.Month, 1);
			while (start < to)
			{
				DateTime end = start.AddMonths(1);
				yield return (start, end);
				start = end;
			}
		}
	}
}
