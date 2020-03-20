using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;

namespace Repositorch.Web.Metrics
{
	public class Files : Metric
	{
		public override object Calculate(IRepository repository, JObject input)
		{
			var revision = repository.GetReadOnly<Commit>()
				.OrderByDescending(x => x.OrderedNumber)
				.First().Revision;
			var files = repository.SelectionDSL()
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
				 let code = repository.SelectionDSL()
						 .Files().ExistInRevision(revision).PathEndsWith(ext)
						 .Modifications().InFiles()
						 .CodeBlocks().InModifications().Fixed()
				 let extFilesCount = code.Files().Again().Count()
				 select new
				 {
					 name = ext,
					 files = string.Format("{0} ({1})",
							 extFilesCount,
							 ((double)extFilesCount / filesCount * 100).ToString("F02")),
					 locAdded = code.Added().CalculateLOC(),
					 locRemoved = -code.Removed().CalculateLOC(),
					 locRemain = code.Added().CalculateLOC() + code.ModifiedBy().CalculateLOC()
				 }).ToArray();

			var dirs =
				(from dir in directories
				 let code = repository.SelectionDSL()
					 .Files().InDirectory(dir).ExistInRevision(revision)
					 .Modifications().InFiles()
					 .CodeBlocks().InModifications().Fixed()
				 let dirFilesCount = code.Files().Again().Count()
				 select new
				 {
					 name = dir,
					 files = string.Format("{0} ({1})",
						 dirFilesCount,
						 ((double)dirFilesCount / filesCount * 100).ToString("F02")
					 ),
					 locAdded = code.Added().CalculateLOC(),
					 locRemoved = -code.Removed().CalculateLOC(),
					 locRemain = code.Added().CalculateLOC() + code.ModifiedBy().CalculateLOC()
				 }).ToArray();

			return new { exts, dirs };
		}
	}
}
