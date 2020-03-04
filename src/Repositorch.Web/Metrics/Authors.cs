﻿using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Metrics
{
	public class Authors : Metric
	{
		protected override object Calculate(ISession s, JObject input)
		{
			string revision = s.GetReadOnly<Commit>()
				.OrderByDescending(x => x.OrderedNumber).First().Revision;
			int commits = s.GetReadOnly<Commit>().Count();
			var authorNames = s.GetReadOnly<Author>()
				.Select(x => x.Name).ToArray();
			double totalLoc = s.SelectionDSL()
				.CodeBlocks().CalculateLOC();
			int totalFiles = s.SelectionDSL()
				.Files().ExistInRevision(revision)
				.Count();

			var codeByAuthor = (from authorName in authorNames select new
			{
				Name = authorName,
				AddedCode = s.SelectionDSL()
					.Authors().NameIs(authorName)
					.Commits().ByAuthors()
					.CodeBlocks().AddedInitiallyInCommits()
					.Fixed(),
				RemovedCode = s.SelectionDSL()
					.Authors().NameIs(authorName)
					.Commits().ByAuthors()
					.Modifications().InCommits()
					.CodeBlocks().InModifications().Removed()
					.Fixed(),
				TouchedFiles = s.SelectionDSL()
					.Authors().NameIs(authorName)
					.Commits().ByAuthors()
					.Files().ExistInRevision(revision).TouchedInCommits()
			}).ToList();

			var authors =
				(from a in codeByAuthor
				 let authorCommits = a.AddedCode.Commits().Again().Count()
				 let authorFixes = a.AddedCode.Commits().Again().AreBugFixes().Count()
				 let authorRefactorings = a.AddedCode.Commits().Again().AreRefactorings().Count()
				 let authorAddedLoc = a.AddedCode.CalculateLOC()
				 let authorCurrentLoc = authorAddedLoc + a.AddedCode.ModifiedBy().CalculateLOC()
				 let authorTouchedFiles = a.TouchedFiles.Count()
				 let authorFilesTouchedByOtherAuthors = a.TouchedFiles
					 .Authors().NameIs(a.Name)
					 .Commits().NotByAuthors()
					 .Files().Again().TouchedInCommits().Count()
				 select new
				 {
					 name = a.Name,
					 commits = string.Format("{0} ({1})", authorCommits, (((double)authorCommits / commits) * 100).ToString("F02")),
					 fixes = string.Format("{0} ({1})", authorFixes, (((double)authorFixes / authorCommits) * 100).ToString("F02")),
					 refactorings = string.Format("{0} ({1})", authorRefactorings, (((double)authorRefactorings / authorCommits) * 100).ToString("F02")),
					 locAdded = a.AddedCode.CalculateLOC(),
					 locRemoved = -a.RemovedCode.CalculateLOC(),
					 locRemain = authorCurrentLoc,
					 contribution = ((authorCurrentLoc / totalLoc) * 100).ToString("F02"),
					 specialization = ((double)authorTouchedFiles / totalFiles * 100).ToString("F02"),
					 uniqueSpecialization = (authorTouchedFiles > 0 ?
						 ((double)(authorTouchedFiles - authorFilesTouchedByOtherAuthors) / totalFiles * 100)
						 :
						 0).ToString("F02"),
					 demandForCode = (authorAddedLoc > 0 ?
						 ((authorCurrentLoc / authorAddedLoc) * 100)
						 :
						 0).ToString("F02")
				 }).OrderBy(x => x.name).ToArray();

			return authors;
		}
	}
}
