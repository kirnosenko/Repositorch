using System;
using System.Collections.Generic;
using System.Text;

namespace Repositorch.Data.Entities
{
	public class Commit
	{
		public int Id { get; set; }

		public int BranchId { get; set; }
		public Branch Branch { get; set; }
	}
}
