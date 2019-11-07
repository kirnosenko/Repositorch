using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.VersionControl
{
	public abstract class Log
	{
		protected List<TouchedFile> touchedFiles;

		public string Revision { get; protected set; }
		public string AuthorName { get; protected set; }
		public string AuthorEmail { get; protected set; }
		public DateTime Date { get; protected set; }
		public string Message { get; protected set; }
		public IEnumerable<string> ParentRevisions { get; protected set; }
		public IEnumerable<string> ChildRevisions { get; protected set; }

		/// <summary>
		/// Returns alphabetically sorted list of touched files.
		/// </summary>
		public IEnumerable<TouchedFile> TouchedFiles
		{
			get { return touchedFiles; }
		}
		/// <summary>
		/// Is this a merge commit with multiple parents.
		/// </summary>
		public bool IsMerge
		{
			get { return ParentRevisions.Count() > 1; }
		}
		/// <summary>
		/// Is this a split commit with multiple children.
		/// </summary>
		public bool IsSplit
		{
			get { return ChildRevisions.Count() > 1; }
		}
		/// <summary>
		/// Is this the very first revision or a no-parent revision
		/// </summary>
		public bool IsOrphan
		{
			get { return ParentRevisions.Count() == 0; }
		}
	}
}
