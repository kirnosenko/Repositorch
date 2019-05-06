using System;
using System.Collections.Generic;
using System.Text;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// VCS commit.
	/// </summary>
	public class Commit
	{
		public int Id { get; set; }
		public int OrderedNumber { get; set; }
		public string Revision { get; set; }
		public string Author { get; set; }
		public string Message { get; set; }
		public DateTime Date { get; set; }

		//public int AuthorId { get; set; }
		//public Author Author { get; set; }

		//public int BranchId { get; set; }
		//public Branch Branch { get; set; }
	}
}
