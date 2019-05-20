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
	[Route("/authors")]
	public class AuthorsController : Controller
	{
		private IDataStore data;

		public AuthorsController(IDataStore data)
		{
			this.data = data;
		}
		public IActionResult Index()
		{
			using (var s = data.OpenSession())
			{
				Dictionary<string, object> result = new Dictionary<string, object>();

				int commits = s.Get<Commit>().Count();
				var authors = s.Get<Author>().ToArray();
				double totalLoc = s.SelectionDSL()
					.CodeBlocks().CalculateLOC();
				int totalFiles = s.SelectionDSL()
					.Files().Exist()
					.Count();

				var codeByAuthor = (from author in authors
					select new
					{
						Name = author.Name,
						AddedCode = s.SelectionDSL()
							.Authors().NameIs(author.Name)
							.Commits().ByAuthors()
							.CodeBlocks().AddedInitiallyInCommits()
							.Fixed(),
						RemovedCode = s.SelectionDSL()
							.Authors().NameIs(author.Name)
							.Commits().ByAuthors()
							.Modifications().InCommits()
							.CodeBlocks().InModifications().Deleted()
							.Fixed(),
						TouchedFiles = s.SelectionDSL()
							.Authors().NameIs(author.Name)
							.Commits().ByAuthors()
							.Files().Exist().TouchedInCommits()
					}).ToList();

				ViewBag.Authors =
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
						commits = string.Format("{0} ({1}%)", authorCommits, (((double)authorCommits / commits) * 100).ToString("F02")),
						fixes = string.Format("{0} ({1}%)", authorFixes, (((double)authorFixes / authorCommits) * 100).ToString("F02")),
						refactorings = string.Format("{0} ({1}%)", authorRefactorings, (((double)authorRefactorings / authorCommits) * 100).ToString("F02")),
						addedLoc = a.AddedCode.CalculateLOC(),
						removedLoc = -a.RemovedCode.CalculateLOC(),
						remainLoc = authorCurrentLoc,
						contribution = ((authorCurrentLoc / totalLoc) * 100).ToString("F02") + "%",
						specialization = ((double)authorTouchedFiles / totalFiles * 100).ToString("F02") + "%",
						uniqueSpecialization = (authorTouchedFiles > 0 ?
							((double)(authorTouchedFiles - authorFilesTouchedByOtherAuthors) / totalFiles * 100)
							:
							0).ToString("F02") + "%",
						demandForCode = (authorAddedLoc > 0 ?
							((authorCurrentLoc / authorAddedLoc) * 100)
							:
							0).ToString("F02") + "%"
					}).OrderBy(x => x.name).ToArray().Select(c => c.ToExpando());
			}
			
			return View();
		}
	}
}
