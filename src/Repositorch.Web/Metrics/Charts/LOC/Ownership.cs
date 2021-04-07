﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;

namespace Repositorch.Web.Metrics.Charts.LOC
{
	public class Ownership : Metric
	{
		private class SettingsIn
		{
			public string path { get; set; }
			public float minimalContribution { get; set; }
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
				minimalContribution = 0.01f,

				dateFrom = new DateTimeOffset(repository.Get<Commit>()
					.Min(x => x.Date)).ToUnixTimeSeconds(),
				dateTo = new DateTimeOffset(repository.Get<Commit>()
					.Max(x => x.Date)).ToUnixTimeSeconds(),
			};
		}

		public override object Calculate(IRepository repository, JObject jsettings)
		{
			var settings = jsettings.ToObject<SettingsIn>();

			var authors = repository.GetReadOnly<Author>().ToArray();
			
			var modifications = string.IsNullOrEmpty(settings.path)
				? repository.GetReadOnly<Modification>()
				: repository.SelectionDSL()
					.Files().PathContains(settings.path)
					.Modifications().InFiles();

			var remainingСodeByAuthor = authors.Select(author => new
			{
				author = author,
				code = (
					from c in repository.GetReadOnly<Commit>()
					join m in modifications on c.Number equals m.CommitNumber
					join cb in repository.GetReadOnly<CodeBlock>() on m.Id equals cb.ModificationId
					join tcb in repository.GetReadOnly<CodeBlock>() on cb.TargetCodeBlockId ?? cb.Id equals tcb.Id
					join tcbc in repository.GetReadOnly<Commit>() on tcb.AddedInitiallyInCommitNumber equals tcbc.Number
					where tcbc.AuthorId == author.Id
					group cb.Size by c.Date.Date into cbc
					select new
					{
						day = cbc.Key,
						locTotal = cbc.Sum()
					}).ToArray()
			}).ToArray();

			var days = remainingСodeByAuthor
				.SelectMany(x => x.code.Select(c => c.day))
				.Distinct()
				.OrderBy(x => x)
				.ToArray();

			var valuableAuthors = new HashSet<string>();
			var loc = days.Select(day =>
			{
				var data = new Dictionary<string, object>()
				{
					{ "date", new DateTimeOffset(day).ToUnixTimeSeconds() },
				};
				var dateCodeSize = remainingСodeByAuthor
					.Sum(a => a.code.Where(x => x.day <= day).Sum(x => x.locTotal));
				var undetalizedDateCodeSize = dateCodeSize;
				foreach (var author in authors)
				{
					var authorCodeSize = remainingСodeByAuthor
						.Single(x => x.author == author).code
						.Where(x => x.day <= day)
						.Sum(x => x.locTotal);
					if (authorCodeSize > 0 &&
						authorCodeSize / dateCodeSize >= settings.minimalContribution)
					{
						data.Add(author.Name, authorCodeSize);
						valuableAuthors.Add(author.Name);
						undetalizedDateCodeSize -= authorCodeSize;
					}
				}
				if (undetalizedDateCodeSize > 0)
				{
					data.Add("Others", undetalizedDateCodeSize);
					valuableAuthors.Add("Others");
				}
				return data;
			}).ToArray();

			return new
			{
				keys = valuableAuthors,
				values = loc,
			};
		}
	}
}
