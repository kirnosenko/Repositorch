using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Repositorch.Data;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Selection.Metrics;
using Repositorch.Web.Models;

namespace Repositorch.Web.Controllers
{
	[Route("/files")]
	public class FilesController : Controller
	{
		private IDataStore data;

		public FilesController(IDataStore data)
		{
			this.data = data;
		}
		public IActionResult Index()
		{
			using (var s = data.OpenSession())
			{
				var files = s.SelectionDSL()
					.Files().Exist()
					.Select(f => f.Path).ToArray();
				var filesCount = files.Count();
				var extensions = files
					.Select(x => Path.GetExtension(x))
					.Where(x => !string.IsNullOrEmpty(x))
					.Distinct();
				
				ViewBag.Extensions =
					(from ext in extensions
					let code = s.SelectionDSL()
							.Files().Exist().PathEndsWith(ext)
							.Modifications().InFiles()
							.CodeBlocks().InModifications().Fixed()
					let extFilesCount = code.Files().Again().Count()
					select new {
						name = ext,
						files = string.Format("{0} ({1}%)",
								extFilesCount,
								((double)extFilesCount / filesCount * 100).ToString("F02")),
						addedLoc = code.Added().CalculateLOC(),
						removedLoc = -code.Removed().CalculateLOC(),
						remainLoc = code.Added().CalculateLOC() + code.ModifiedBy().CalculateLOC()
					}).ToArray().Select(c => c.ToExpando());

				var dirs = files
					.Select(x => Path.GetDirectoryName(x).Replace("\\", "/"))
					.Distinct();
				
				ViewBag.Directories =
					(from dir in dirs
					let code = s.SelectionDSL()
						.Files().InDirectory(dir).Exist()
						.Modifications().InFiles()
						.CodeBlocks().InModifications().Fixed()
					let dirFilesCount = code.Files().Again().Count()
					select new {
						name = dir,
						files = string.Format("{0} ({1}%)",
							dirFilesCount,
							((double)dirFilesCount / filesCount * 100).ToString("F02")
						),
						addedLoc = code.Added().CalculateLOC(),
						removedLoc = -code.Removed().CalculateLOC(),
						remainLoc = code.Added().CalculateLOC() + code.ModifiedBy().CalculateLOC()
					}).ToArray().Select(c => c.ToExpando());
			}

			return View();
		}
	}
}
