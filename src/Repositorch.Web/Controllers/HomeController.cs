using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;
using Repositorch.Web.Models;

namespace Repositorch.Web.Controllers
{
	public class HomeController : Controller
	{
		private IDataStore data;

		public HomeController(IDataStore data)
		{
			this.data = data;
		}
		public IActionResult Index()
		{
			using (var s = data.OpenSession())
			{
				int commitsCount = s.Get<Commit>().Count();
				int commitsFixCount = s.SelectionDSL().Commits().AreBugFixes().Count();
				string commitsFixPercent = ((double)commitsFixCount / commitsCount * 100).ToString("F02");
				int commitsRefactoringCount = s.SelectionDSL().Commits().AreRefactorings().Count();
				string commitsRefactoringPercent = ((double)commitsRefactoringCount / commitsCount * 100).ToString("F02");

				DateTime periodFrom = s.Get<Commit>().Min(x => x.Date);
				DateTime periodTo = s.Get<Commit>().Max(x => x.Date);

				ViewData["periodFrom"] = periodFrom;
				ViewData["periodTo"] = periodTo;
				ViewData["periodDays"] = (periodTo - periodFrom).Days;
				ViewData["periodYears"] = ((periodTo - periodFrom).TotalDays / 365).ToString("F01");
				ViewData["authors"] = s.Get<Author>().Count();

				ViewData["commits"] = commitsCount;
				ViewData["commitsFix"] = commitsFixCount;
				ViewData["commitsFixPercent"] = commitsFixPercent;
				ViewData["commitsRefactoring"] = commitsRefactoringCount;
				ViewData["commitsRefactoringPercent"] = commitsRefactoringPercent;
				
				var files = s.SelectionDSL()
					.Files().Fixed();
				ViewData["files"] = files.Exist().Count();
				ViewData["filesAdded"] = files.Count();
				ViewData["filesRemoved"] = files.Deleted().Count();

				var code = s.SelectionDSL()
					.CodeBlocks().Fixed();
				ViewData["loc"] = code.CalculateLOC();
				ViewData["locAdded"] = code.Added().CalculateLOC();
				ViewData["locRemoved"] = -code.Deleted().CalculateLOC();
			}

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
