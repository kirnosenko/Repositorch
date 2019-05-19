using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;
using Repositorch.Web.Models;

namespace Repositorch.Web.Controllers
{
	[Route("/activity")]
	public class ActivityController : Controller
    {
		private IDataStore data;

		public ActivityController(IDataStore data)
		{
			this.data = data;
		}
		public IActionResult Index()
        {
			using (var s = data.OpenSession())
			{
				double totalLoc = s.SelectionDSL()
					.CodeBlocks().CalculateLOC();

				var periods = SplitPeriod(
					s.Get<Commit>().Min(x => x.Date),
					s.Get<Commit>().Max(x => x.Date));
					
				ViewBag.Periods =
					(from period in periods
					let commits = s.SelectionDSL().Commits()
						.DateIsGreaterOrEquelThan(period.start)
						.DateIsLesserThan(period.end)
						.Fixed()
					let code = commits
						.Modifications().InCommits()
						.CodeBlocks().InModifications().Fixed()
					let totalCommits = s.SelectionDSL().Commits()
						.DateIsLesserThan(period.end)
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
					select new {
						title = string.Format("{0}-{1:00}", period.start.Year, period.start.Month),
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
							-code.Deleted().CalculateLOC(),
							-totalCode.Deleted().CalculateLOC()
						),
						loc = totalCode.CalculateLOC()
					}).OrderBy(x => x.title).ToArray().Select(c => c.ToExpando());
			}

			return View();
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