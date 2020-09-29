using System;
using Repositorch.Data.Entities;

namespace Repositorch.Web.Projects
{
	public class ProjectData
	{
		public ProjectSettings Settings { get; set; }
		public Commit[] Commits { get; set; }
		public Tag[] Tags { get; set; }
		public Author[] Authors { get; set; }
		public Branch[] Branches { get; set; }
		public BugFix[] Fixes { get; set; }
		public CodeFile[] Files { get; set; }
		public Modification[] Modifications { get; set; }
		public CodeBlock[] Blocks { get; set; }
	}
}
