using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Web.Metrics.Charts.LOC
{
	public class Burndown : Metric
	{
		private enum SliceType
		{
			YEAR,
			TAG
		}

		private struct Slice
		{
			public string Label;
			public Expression<Func<Commit,bool>> Check;
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
				slice = SliceType.YEAR,

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
				? GetYearSlices(repository)
				: GetTagSlices(repository);

			var modifications = string.IsNullOrEmpty(settings.path)
				? repository.GetReadOnly<Modification>()
				: repository.SelectionDSL()
					.Files().PathContains(settings.path)
					.Modifications().InFiles();

			var remainingСodeByDate = slices.Select(slice => new
			{
				label = slice.Label,
				code = (
					from c in repository.Get<Commit>()
					join m in modifications on c.Id equals m.CommitId
					join cb in repository.Get<CodeBlock>() on m.Id equals cb.ModificationId
					join tcb in repository.Get<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
					join tcbc in repository.Get<Commit>().Where(slice.Check) on tcb.AddedInitiallyInCommitId equals tcbc.Id
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
						{ "date", new DateTimeOffset(date).ToUnixTimeSeconds() },
					};
					foreach (var slice in slices)
					{
						var codeSize = remainingСodeByDate
							.Single(x => x.label == slice.Label).code
							.Where(x => x.date <= date)
							.Sum(x => x.locTotal);
						if (codeSize > 0)
						{
							data.Add(slice.Label, codeSize);
						}
					}
					return data;
				}).ToArray();

			return new
			{
				keys = slices.Select(x => x.Label).ToArray(),
				values = loc,
			};
		}

		private Slice[] GetYearSlices(IRepository repository)
		{
			var dateStart = repository.Get<Commit>().Min(x => x.Date);
			var dateStop = repository.Get<Commit>().Max(x => x.Date);

			List<Slice> slices = new List<Slice>();
			while (dateStart < dateStop)
			{
				var dateFrom = dateStart;
				var dateTo = new DateTime(dateStart.Year + 1, 1, 1, 0, 0, 0);
				slices.Add(new Slice()
				{
					Label = dateStart.Year.ToString(),
					Check = c => c.Date.Year == dateFrom.Year
				});
				dateStart = dateTo;
			}

			return slices.ToArray();
		}

		private Slice[] GetTagSlices(IRepository repository)
		{
			var tags = (
				from t in repository.Get<Tag>()
				join c in repository.Get<Commit>() on t.CommitId equals c.Id
				select new
				{
					tag = t.Title,
					date = c.Date
				}).OrderBy(x => x.date).ToArray();
				
			List<Slice> slices = new List<Slice>();
			DateTime from = DateTime.MinValue;
			foreach (var tag in tags)
			{
				var tagFrom = from;
				slices.Add(new Slice()
				{
					Label = tag.tag,
					Check = c => c.Date > tagFrom && c.Date <= tag.date
				});
				from = tag.date;
			}

			return slices.ToArray();
		}
	}
}
