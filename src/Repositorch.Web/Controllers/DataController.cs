using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities.Persistent;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DataController : ControllerBase
	{
		private IDataStore data;

		public DataController()
		{
			data = new SqlServerDataStore("git");
		}

		[HttpGet]
		[Route("CalculateMetrics/{name}")]
		public ActionResult<JObject> CalculateMetrics([FromRoute]string name)
		{
			return CalculateMetrics(name, null);
		}

		[HttpPost]
		[Route("CalculateMetrics/{name}")]
		public ActionResult<JObject> CalculateMetrics([FromRoute]string name, [FromBody]JObject input)
		{
			switch (name)
			{
				case "Summary":
					return GetSummary();
				case "Authors":
					return GetAuthors();
				case "Files":
					return GetFiles();
				case "Activity":
					return GetActivity();
				default:
					return BadRequest();
			}
		}

		private ActionResult<JObject> GetSummary()
		{
			using (var s = data.OpenSession())
			{
				var revision = s.GetReadOnly<Commit>()
					.OrderByDescending(x => x.OrderedNumber).First().Revision;
				int commitsCount = s.GetReadOnly<Commit>().Count();
				int commitsFixCount = s.SelectionDSL().Commits().AreBugFixes().Count();
				string commitsFixPercent = ((double)commitsFixCount / commitsCount * 100).ToString("F02");
				int commitsRefactoringCount = s.SelectionDSL().Commits().AreRefactorings().Count();
				string commitsRefactoringPercent = ((double)commitsRefactoringCount / commitsCount * 100).ToString("F02");

				DateTime periodFrom = s.GetReadOnly<Commit>().Min(x => x.Date);
				DateTime periodTo = s.GetReadOnly<Commit>().Max(x => x.Date);

				var files = s.SelectionDSL()
					.Files().Fixed();
				var filesAll = files.Count();
				var filesExist = files.ExistInRevision(revision).Count();
				var filesRemoved = filesAll - filesExist;

				var code = s.SelectionDSL()
					.CodeBlocks().Fixed();

				var result = new
				{
					PeriodFrom = periodFrom.ToShortDateString(),
					PeriodTo = periodTo.ToShortDateString(),
					PeriodDays = (periodTo - periodFrom).Days,
					PeriodYears = ((periodTo - periodFrom).TotalDays / 365).ToString("F01"),

					Authors = s.Get<Author>().Count(),

					Commits = commitsCount,
					CommitsFix = commitsFixCount,
					CommitsFixPercent = commitsFixPercent,
					CommitsRefactoring = commitsRefactoringCount,
					CommitsRefactoringPercent = commitsRefactoringPercent,

					Files = filesExist,
					FilesAdded = filesAll,
					FilesRemoved = filesRemoved,

					Loc = code.CalculateLOC(),
					LocAdded = code.Added().CalculateLOC(),
					LocRemoved = -code.Removed().CalculateLOC(),
				};

				return Ok(result);
			}
		}

		private ActionResult<JObject> GetAuthors()
		{
			using (var s = data.OpenSession())
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
					 }).OrderBy(x => x.name).ToArray();

				return Ok(new { authors });
			}
		}

		private ActionResult<JObject> GetFiles()
		{
			using (var s = data.OpenSession())
			{
				var revision = s.GetReadOnly<Commit>()
					.OrderByDescending(x => x.OrderedNumber).First().Revision;
				var files = s.SelectionDSL()
					.Files().ExistInRevision(revision)
					.Select(f => f.Path).ToArray();
				var filesCount = files.Count();
				var extensions = files
					.Select(x => Path.GetExtension(x))
					.Where(x => !string.IsNullOrEmpty(x))
					.Distinct();
				var directories = files
					.Select(x => Path.GetDirectoryName(x).Replace("\\", "/"))
					.Distinct();

				var exts =
					(from ext in extensions
					 let code = s.SelectionDSL()
							 .Files().ExistInRevision(revision).PathEndsWith(ext)
							 .Modifications().InFiles()
							 .CodeBlocks().InModifications().Fixed()
					 let extFilesCount = code.Files().Again().Count()
					 select new
					 {
						 name = ext,
						 files = string.Format("{0} ({1}%)",
								 extFilesCount,
								 ((double)extFilesCount / filesCount * 100).ToString("F02")),
						 addedLoc = code.Added().CalculateLOC(),
						 removedLoc = -code.Removed().CalculateLOC(),
						 remainLoc = code.Added().CalculateLOC() + code.ModifiedBy().CalculateLOC()
					 }).ToArray();

				var dirs =
					(from dir in directories
					 let code = s.SelectionDSL()
						 .Files().InDirectory(dir).ExistInRevision(revision)
						 .Modifications().InFiles()
						 .CodeBlocks().InModifications().Fixed()
					 let dirFilesCount = code.Files().Again().Count()
					 select new
					 {
						 name = dir,
						 files = string.Format("{0} ({1}%)",
							 dirFilesCount,
							 ((double)dirFilesCount / filesCount * 100).ToString("F02")
						 ),
						 addedLoc = code.Added().CalculateLOC(),
						 removedLoc = -code.Removed().CalculateLOC(),
						 remainLoc = code.Added().CalculateLOC() + code.ModifiedBy().CalculateLOC()
					 }).ToArray();

				return Ok(new { exts, dirs });
			}
		}

		private ActionResult<JObject> GetActivity()
		{
			using (var s = data.OpenSession())
			{
				double totalLoc = s.SelectionDSL()
					.CodeBlocks().CalculateLOC();

				var periodFrames = SplitPeriod(
					s.Get<Commit>().Min(x => x.Date),
					s.Get<Commit>().Max(x => x.Date));

				var periods =
					(from frame in periodFrames
					 let commits = s.SelectionDSL().Commits()
						 .FromDate(frame.start)
						 .BeforeDate(frame.end)
						 .Fixed()
					 let code = commits
						 .Modifications().InCommits()
						 .CodeBlocks().InModifications().Fixed()
					 let totalCommits = s.SelectionDSL().Commits()
						 .BeforeDate(frame.end)
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
					 select new
					 {
						 title = string.Format("{0}-{1:00}", frame.start.Year, frame.start.Month),
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
							 -code.Removed().CalculateLOC(),
							 -totalCode.Removed().CalculateLOC()
						 ),
						 loc = totalCode.CalculateLOC()
					 }).OrderBy(x => x.title).ToArray();

				return Ok(new { periods });
			}
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
