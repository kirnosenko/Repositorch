using System;
using System.Collections.Generic;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Author of commits in VCS.
	/// </summary>
	public class Author
	{
		public int Id { get; set; }
		/// <summary>
		/// Author name which presented in VCS log.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Author email.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Commits by the author.
		/// </summary>
		public List<Commit> Commits { get; set; } = new List<Commit>();
	}
}
