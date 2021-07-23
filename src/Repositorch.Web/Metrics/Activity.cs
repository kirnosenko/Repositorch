using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.Selection;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Metrics
{
	public class Activity : Metric
	{
		private enum SliceType
		{
			WEEK,
			MONTH,
			YEAR,
			TAG
		}

		private class SettingsIn
		{
			public string path { get; set; }
			public SliceType slice { get; set; }
		}

		private class SettingsOut : SettingsIn
		{
			public long dateFrom { get; set; }
			public long dateTo { get; set; }
		}

		public override object GetSettings(IRepository repository)
		{
			return new SettingsOut()
			{
				path = string.Empty,
				slice = SliceType.MONTH,

				dateFrom = new DateTimeOffset(repository.Get<Commit>()
					.Min(x => x.Date)).ToUnixTimeSeconds(),
				dateTo = new DateTimeOffset(repository.Get<Commit>()
					.Max(x => x.Date)).ToUnixTimeSeconds(),
			};
		}

		public override object Calculate(IRepository repository, JObject jsettings)
		{
			var settings = jsettings.ToObject<SettingsIn>();
			var slices = settings.slice == SliceType.YEAR
				? repository.GetYearSlices()
				: settings.slice == SliceType.MONTH
					? repository.GetMonthSlices()
					: settings.slice == SliceType.WEEK
						? repository.GetWeekSlices(DayOfWeek.Monday)
						: repository.GetTagSlices();

			var modifications = string.IsNullOrEmpty(settings.path)
				? repository.GetReadOnly<Modification>()
				: repository.SelectionDSL()
					.Files().PathContains(settings.path)
					.Modifications().InFiles();

			var periods = slices.Select(slice =>
			{
				var commits = repository.SelectionDSL().Commits()
					.Reselect(s => s.Where(slice.Condition))
					.Fixed();
				var code = commits
					.Files().Reselect(exp => string.IsNullOrEmpty(settings.path)
						? exp
						: exp.PathContains(settings.path))
					.Modifications().InCommits().Reselect(exp => string.IsNullOrEmpty(settings.path) 
						? exp
						: exp.InFiles())
					.CodeBlocks().InModifications().Fixed();
				var totalCommits = repository.SelectionDSL().Commits()
					.Reselect(s => s.Where(slice.EndCondition))
					.Fixed();
				var totalCode = totalCommits
					.Files().Reselect(exp => string.IsNullOrEmpty(settings.path)
						? exp
						: exp.PathContains(settings.path))
					.Modifications().InCommits().Reselect(exp => string.IsNullOrEmpty(settings.path)
						? exp
						: exp.InFiles())
					.CodeBlocks().InModifications().Fixed();
				var commitsCount = commits.Count();
				var totalCommitsCount = totalCommits.Count();
				var authorsCount = commits.
					Authors().OfCommits().Count();
				var totalAuthorsCount = totalCommits
					.Authors().OfCommits().Count();
				var lastRevision = totalCommits
					.OrderByDescending(x => x.Number)
					.First().Revision;

				return new
				{
					period = slice.Label,
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
				};
			}).ToArray();

			return periods;
		}
	}
}
