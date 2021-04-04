using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Metrics
{
	public class Activity : Metric
	{
		private enum SliceType
		{
			DAY,
			WEEK,
			MONTH,
			YEAR,
			TAG
		}

		public override object Calculate(IRepository repository, JObject settings)
		{
			double totalLoc = repository.SelectionDSL()
				.CodeBlocks().CalculateLOC();

			var periodFrames = SplitPeriod(
				repository.GetReadOnly<Commit>().Min(x => x.Date),
				repository.GetReadOnly<Commit>().Max(x => x.Date)).ToArray();

			var periods =
				(from frame in periodFrames
				 let commits = repository.SelectionDSL().Commits()
					 .FromDate(frame.start)
					 .BeforeDate(frame.end)
					 .Fixed()
				 let code = commits
					 .Modifications().InCommits()
					 .CodeBlocks().InModifications().Fixed()
				 let totalCommits = repository.SelectionDSL().Commits()
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
				 let lastRevision = totalCommits
					 .OrderByDescending(x => x.Date)
					 .First().Revision
				 select new
				 {
					 period = string.Format("{0}-{1:00}", frame.start.Year, frame.start.Month),
					 commits = string.Format("{0} ({1})",
						 commitsCount,
						 totalCommitsCount
					 ),
					 authors = string.Format("{0} ({1})",
						 authorsCount,
						 totalAuthorsCount
					 ),
					 files = totalCommits.Files().AddedInCommits().Count() -
						totalCommits.Files().RemovedInCommits().Count(),
					 defectsFixed = string.Format("{0} ({1})",
						 commits.AreBugFixes().Count(),
						 totalCommits.AreBugFixes().Count()
					 ),
					 locAdded = string.Format("{0} ({1})",
						 code.Added().CalculateLOC(),
						 totalCode.Added().CalculateLOC()
					 ),
					 locRemoved = string.Format("{0} ({1})",
						 -code.Removed().CalculateLOC(),
						 -totalCode.Removed().CalculateLOC()
					 ),
					 locRemain = totalCode.CalculateLOC()
				 }).OrderBy(x => x.period).ToArray();

			return periods;
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
