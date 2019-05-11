using System;
using System.Collections.Generic;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Author that made commits.
	/// </summary>
	public class Author
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }

		public List<Commit> Commits { get; set; } = new List<Commit>();
	}
}
