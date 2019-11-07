using System;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitData : IVcsData
	{
		private IGitClient git;
		private GitRevisions revisions;
		
		public GitData(IGitClient git)
		{
			this.git = git;
			revisions = new GitRevisions(git.RevList());
		}

		public string GetRevisionByNumber(int number)
		{
			return revisions.GetRevisionByNumber(number);
		}
		
		public Log Log(string revision)
		{
			using (var log = git.Log(revision))
			{
				var revisionNode = revisions.GetRevisionNode(revision);
				return new GitLog(log, revisionNode.Parents, revisionNode.Children);
			}
		}
		public IBlame Blame(string revision, string filePath)
		{
			using (var blame = git.Blame(revision, filePath))
			{
				return GitBlame.Parse(blame);
			}
		}
	}
}
