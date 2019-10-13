using System;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	public class CommandLineGitClient : CommandLineTool, IGitClient
	{
		public CommandLineGitClient(string repositoryPath)
		{
			RepositoryPath = repositoryPath;
			GitCommand = "git";
			Branch = "master";
		}
		public Stream RevList()
		{
			return RunCommand(
				"rev-list {0} --topo-order --reverse --parents",
				Branch
			);
		}
		public Stream Log(string revision)
		{
			return RunCommand(
				"log -n 1 -C -m --format=format:%H%n%cn%n%ce%n%ci%n%s --name-status {0}",
				revision
			);
		}
		public Stream Blame(string revision, string filePath)
		{
			return RunCommand(
				"blame -l -s --root --incremental {0} -- {1}",
				revision, ToGitPath(filePath)
			);
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
