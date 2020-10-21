using System;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	public class CommandLineGitClient : CommandLineTool, IVcsData
	{
		private GitRevisionGraph revisions;

		public CommandLineGitClient(string repositoryPath)
		{
			RepositoryPath = repositoryPath;
			GitCommand = "git";
			Branch = "master";
			ExtendedLog = false;
		}

		public string GetRevisionByNumber(int number)
		{
			if (revisions == null)
			{
				revisions = new GitRevisionGraph(GetRevList());
			}

			return revisions.GetRevisionByNumber(number);
		}
		public Log Log(string revision)
		{
			if (revisions == null)
			{
				revisions = new GitRevisionGraph(GetRevList());
			}

			using (var log = GetLog(revision, ExtendedLog))
			{
				var revisionNode = revisions.GetRevisionNode(revision);
				if (!ExtendedLog)
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
		public bool ExtendedLog
		{
			get; set;
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
				"log -n 1 -C -m --format=format:%H%n%cn%n%ce%n%s%n%n%ci%n%D --encoding=UTF-8 {0} {1}",
				extended ? "--numstat --summary" : "--name-status",
				revision
			);
		}
		private Stream GetBlame(string revision, string filePath)
		{
			return RunCommand(
				"blame -l -s --root --incremental {0} -- \"{1}\"",
				revision, ToGitPath(filePath)
			);
		}
		private Stream RunCommand(string cmd, params object[] objects)
		{
			return GetCommandOutput(
				GitCommand,
				string.Format("--git-dir=\"{0}\" ", RepositoryPath) + string.Format(cmd, objects));
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
