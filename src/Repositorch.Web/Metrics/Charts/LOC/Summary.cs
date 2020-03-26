using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Web.Metrics.Charts.LOC
{
	public class Summary : Metric
	{
		private class Settings
		{
			public bool locTotal { get; set; }
			public bool locAdded { get; set; }
			public bool locRemoved { get; set; }
			public string author { get; set; }
			public string path { get; set; }
			public long dateFrom { get; set; }
			public long dateTo { get; set; }
		}

		public override object GetDefaultSettings(IRepository repository)
		{
			return new Settings()
			{
				locTotal = true,
				locAdded = false,
				locRemoved = false,
				author = string.Empty,
				path = string.Empty,
				dateFrom = new DateTimeOffset(repository.GetReadOnly<Commit>()
					.Min(x => x.Date)).ToUnixTimeSeconds(),
				dateTo = new DateTimeOffset(repository.GetReadOnly<Commit>()
					.Max(x => x.Date)).ToUnixTimeSeconds()
			};
		}
		public override object GetFormData(IRepository repository)
		{
			return new
			{
				authors = repository.GetReadOnly<Author>()
					.Select(x => x.Name)
					.ToArray(),
			};
		}
		public override object Calculate(IRepository repository, JObject jsettings)
		{
			var settings = jsettings.ToObject<Settings>();

			var commits = string.IsNullOrEmpty(settings.author)
				? repository.GetReadOnly<Commit>()
				: repository.SelectionDSL()
					.Authors().NameIs(settings.author)
					.Commits().ByAuthors();

			var modifications = string.IsNullOrEmpty(settings.path)
				? repository.GetReadOnly<Modification>()
				: repository.SelectionDSL()
					.Files().PathContains(settings.path)
					.Modifications().InFiles();

			var codeByDate = (
				from c in commits
				join m in modifications on c.Id equals m.CommitId
				join cb in repository.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
				group cb.Size by c.Date into cbc
				select new
				{
					date = cbc.Key,
					locTotal = cbc.Sum(),
					locAdded = cbc.Sum(x => x > 0 ? x : 0),
					locRemoved = cbc.Sum(x => x < 0 ? -x : 0),
				}).ToArray();

			var loc = codeByDate.Select(c => new
			{
				date = c.date,
				locTotal = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.locTotal),
				locAdded = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.locAdded),
				locRemoved = codeByDate
					.Where(x => x.date <= c.date)
					.Sum(x => x.locRemoved),
			}).OrderBy(x => x.date).ToArray();

			return loc;
		}
	}
}
