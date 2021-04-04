using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.Selection;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Web.Metrics.Charts.LOC
{
	public class Burndown : Metric
	{
		private enum SliceType
		{
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
				? repository.GetYearSlices()
				: settings.slice == SliceType.MONTH
					? repository.GetMonthSlices()
					: repository.GetTagSlices();

			var modifications = string.IsNullOrEmpty(settings.path)
				? repository.GetReadOnly<Modification>()
				: repository.SelectionDSL()
					.Files().PathContains(settings.path)
					.Modifications().InFiles();

			var remainingСodeByDay = slices.Select(slice => new
			{
				label = slice.Label,
				code = (
					from c in repository.Get<Commit>()
					join m in modifications on c.Number equals m.CommitNumber
					join cb in repository.Get<CodeBlock>() on m.Id equals cb.ModificationId
					join tcb in repository.Get<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
					join tcbc in repository.Get<Commit>().Where(slice.Check) on tcb.AddedInitiallyInCommitNumber equals tcbc.Number
					group cb.Size by c.Date.Date into cbc
					select new
					{
						day = cbc.Key,
						locTotal = cbc.Sum()
					}).ToArray()
			}).ToArray();

			var days = remainingСodeByDay
				.SelectMany(x => x.code.Select(c => c.day))
				.Distinct()
				.OrderBy(x => x)
				.ToArray();

			var loc = days.Select(day =>
			{
				var data = new Dictionary<string, object>()
				{
					{ "date", new DateTimeOffset(day).ToUnixTimeSeconds() },
				};
				foreach (var slice in slices)
				{
					var codeSize = remainingСodeByDay
						.Single(x => x.label == slice.Label).code
						.Where(x => x.day <= day)
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
	}
}
