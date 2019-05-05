using System;
using System.Collections.Generic;
using System.Text;

namespace Repositorch.Data.Entities
{
	public class Branch
	{
		public int Id { get; set; }
		public int Mask { get; set; }
		public int Offset { get; set; }

		public List<Commit> Commits { get; set; }
	}
}
