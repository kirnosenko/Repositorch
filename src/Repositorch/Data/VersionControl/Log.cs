using System;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public abstract class Log
	{
		protected List<TouchedFile> touchedFiles;

		public string Revision { get; protected set; }
		public string Author { get; protected set; }
		public DateTime Date { get; protected set; }
		public string Message { get; protected set; }

		/// <summary>
		/// Returns alphabetically sorted list of touched files.
		/// </summary>
		public IEnumerable<TouchedFile> TouchedFiles
		{
			get { return touchedFiles; }
		}
	}
}
