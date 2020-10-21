using System.IO;
using System.Linq;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitRevisionGraph : RevisionGraph
	{
		public GitRevisionGraph(Stream revList)
		{
			TextReader reader = new StreamReader(revList);
			
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				var lineRevisions = line.Split(' ');
				var revision = lineRevisions.First();
				var parents = lineRevisions.Skip(1).ToArray();

				AddRevision(revision, parents);
			}
		}
	}
}
