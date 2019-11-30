using System;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	public class CommandLineGitClient : CommandLineTool, IVcsData
	{
		private GitRevisions revisions;

		public CommandLineGitClient(string repositoryPath)
		{
			RepositoryPath = repositoryPath;
			GitCommand = "git";
			Branch = "master";
			IgnoreBinary = false;
		}

		public string GetRevisionByNumber(int number)
		{
			if (revisions == null)
			{
				GitConfig();
				revisions = new GitRevisions(GetRevList());
			}

			return revisions.GetRevisionByNumber(number);
		}
		public Log Log(string revision)
		{
			if (revisions == null)
			{
				GitConfig();
				revisions = new GitRevisions(GetRevList());
			}

			using (var log = GetLog(revision, IgnoreBinary))
			{
				var revisionNode = revisions.GetRevisionNode(revision);
				if (!IgnoreBinary)
				{
					return new GitLog(log, revisionNode.Parents, revisionNode.Children);
				}

				return new GitLogExtended(log, revisionNode.Parents, revisionNode.Children);
			}
		}
		public IBlame Blame(string revision, string filePath)
		{
			using (var blame = GetBlame(revision, filePath))
			{
				return GitBlame.Parse(blame);
			}
		}
		public string RepositoryPath
		{
			get; private set;
		}
		public string GitCommand
		{
			get; set;
		}
		public string Branch
		{
			get; set;
		}
		public bool IgnoreBinary
		{
			get; set;
		}

		private void GitConfig()
		{
			RunCommand("config diff.renameLimit 999999");
			RunCommand("config merge.renameLimit 999999");
		}
		private Stream GetRevList()
		{
			return RunCommand(
				"rev-list {0} --topo-order --reverse --parents",
				Branch
			);
		}
		private Stream GetLog(string revision, bool extended)
		{
			return RunCommand(
				"log -n 1 -C -m --format=format:%H%n%cn%n%ce%n%ci%n%s%n%D --encoding=UTF-8 {0} {1}",
				extended ? "--numstat --summary" : "--name-status",
				revision
			);
		}
		private Stream GetBlame(string revision, string filePath)
		{
			return RunCommand(
				"blame -l -s --root --incremental {0} -- {1}",
				revision, ToGitPath(filePath)
			);
		}
		private Stream RunCommand(string cmd, params object[] objects)
		{
			return GetCommandOutput(
				GitCommand,
				string.Format("--git-dir={0} ", RepositoryPath) + string.Format(cmd, objects));
		}
		/// <summary>
		/// Remove leading slash to get git path.
		/// </summary>
		/// <param name="path">Internal path with leading slash.</param>
		/// <returns>Git path.</returns>
		private string ToGitPath(string path)
		{
			return path.Remove(0, 1);
		}
	}
}
