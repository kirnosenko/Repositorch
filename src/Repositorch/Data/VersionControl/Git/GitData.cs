using System;
using System.Collections.Generic;
using System.IO;

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
		public IEnumerable<string> GetRevisionParents(string revision)
		{
			return revisions.GetRevisionParents(revision);
		}
		public IEnumerable<string> GetRevisionChildren(string revision)
		{
			return revisions.GetRevisionChildren(revision);
		}
		
		public Log Log(string revision)
		{
			using (var log = git.Log(revision))
			{
				return new GitLog(log);
			}
		}
		public IDiff Diff(string revision)
		{
			using (var diff = git.Diff(revision))
			{
				return new GitDiff(diff);
			}
		}
		public IBlame Blame(string revision, string filePath)
		{
			using (var blame = git.Blame(revision, filePath))
			{
				return new GitBlame(blame);
			}
		}
	}
}
