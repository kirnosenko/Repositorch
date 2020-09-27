using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Metrics
{
	public class Summary : Metric
	{
		public override object Calculate(IRepository repository, JObject settings)
		{
			var revision = repository.GetReadOnly<Commit>()
				.OrderByDescending(x => x.Number)
				.First().Revision;
			int commitsCount = repository.GetReadOnly<Commit>()
				.Count();
			int commitsFixCount = repository.SelectionDSL()
				.Commits().AreBugFixes().Count();
			string commitsFixPercent = ((double)commitsFixCount / commitsCount * 100).ToString("F02");
			int commitsRefactoringCount = repository.SelectionDSL()
				.Commits().AreRefactorings().Count();
			string commitsRefactoringPercent = ((double)commitsRefactoringCount / commitsCount * 100).ToString("F02");

			DateTime periodFrom = repository.GetReadOnly<Commit>().Min(x => x.Date);
			DateTime periodTo = repository.GetReadOnly<Commit>().Max(x => x.Date);

			var files = repository.SelectionDSL()
				.Files().Fixed();
			var filesAll = files.Count();
			var filesExist = files.ExistInRevision(revision).Count();
			var filesRemoved = filesAll - filesExist;

			var code = repository.SelectionDSL()
				.CodeBlocks().Fixed();

			var summary = new
			{
				PeriodFrom = periodFrom.ToShortDateString(),
				PeriodTo = periodTo.ToShortDateString(),
				PeriodDays = (periodTo - periodFrom).Days,
				PeriodYears = ((periodTo - periodFrom).TotalDays / 365).ToString("F01"),

				Authors = repository.Get<Author>().Count(),

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

			return summary;
		}
	}
}
