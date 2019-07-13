using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// File with code under control of VCS.
	/// </summary>
	public class CodeFile
	{
		public int Id { get; set; }
		/// <summary>
		/// UNIX-formated path of the file with leading slash.
		/// </summary>
		public string Path { get; set; }
	}
}
