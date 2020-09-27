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
		private class SettingsIn
		{
			public string author { get; set; }
			public string path { get; set; }
		}

		private class SettingsOut : SettingsIn
		{
			public bool locTotal { get; set; }
			public bool locAdded { get; set; }
			public bool locRemoved { get; set; }
			public long dateFrom { get; set; }
			public long dateTo { get; set; }
			public string[] authors { get; set; }
		}

		public override object GetSettings(IRepository repository)
		{
			return new SettingsOut()
			{
				author = string.Empty,
				path = string.Empty,

				locTotal = true,
				locAdded = false,
				locRemoved = false,
				dateFrom = new DateTimeOffset(repository.Get<Commit>()
					.Min(x => x.Date)).ToUnixTimeSeconds(),
				dateTo = new DateTimeOffset(repository.Get<Commit>()
					.Max(x => x.Date)).ToUnixTimeSeconds(),
				authors = repository.GetReadOnly<Author>()
					.Select(x => x.Name)
					.ToArray(),
			};
		}

		public override object Calculate(IRepository repository, JObject jsettings)
		{
			var settings = jsettings.ToObject<SettingsIn>();

			int? authorId = string.IsNullOrEmpty(settings.author)
				? (int?)null
				: repository.SelectionDSL()
					.Authors().NameIs(settings.author)
					.SingleOrDefault()?.Id;

			var commits = repository.GetReadOnly<Commit>();
			if (authorId != null)
			{
				commits = commits.Where(x => x.AuthorId == authorId);
			}

			var modifications = string.IsNullOrEmpty(settings.path)
				? repository.GetReadOnly<Modification>()
				: repository.SelectionDSL()
					.Files().PathContains(settings.path)
					.Modifications().InFiles();

			var codeByDate = (
				from c in commits
				join m in modifications on c.Number equals m.CommitNumber
				join cb in repository.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
				group cb.Size by c.Date into cbc
				select new
				{
					date = cbc.Key,
					locTotal = cbc.Sum(),
					locAdded = cbc.Sum(x => x > 0 ? x : 0),
					locRemoved = cbc.Sum(x => x < 0 ? -x : 0),
				}).ToArray();

			var codeByDateForAuthor = authorId == null ? null : (
				from c in repository.GetReadOnly<Commit>()
				join m in modifications on c.Number equals m.CommitNumber
				join cb in repository.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
				join tcb in repository.GetReadOnly<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
				join tcbc in repository.GetReadOnly<Commit>() on tcb.AddedInitiallyInCommitNumber equals tcbc.Number
				where tcbc.AuthorId == authorId
				group cb.Size by c.Date into cbc
				select new
				{
					date = cbc.Key,
					locTotal = cbc.Sum()
				}).ToArray();

			var dates = repository.GetReadOnly<Commit>()
				.Select(x => x.Date)
				.ToArray();

			var loc = dates.Select(date => new
			{
				date = new DateTimeOffset(date).ToUnixTimeSeconds(),
				locTotal = codeByDateForAuthor != null
					? codeByDateForAuthor
						.Where(x => x.date <= date)
						.Sum(x => x.locTotal)
					: codeByDate
						.Where(x => x.date <= date)
						.Sum(x => x.locTotal),
				locAdded = codeByDate
					.Where(x => x.date <= date)
					.Sum(x => x.locAdded),
				locRemoved = codeByDate
					.Where(x => x.date <= date)
					.Sum(x => x.locRemoved),
			}).OrderBy(x => x.date).ToArray();

			return loc;
		}
	}
}
