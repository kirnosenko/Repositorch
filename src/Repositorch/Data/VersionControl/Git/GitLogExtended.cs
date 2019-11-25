using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Repositorch.Data.VersionControl.Git
{
	/// <summary>
	/// Extended version of git log to ignore git-detected
	/// binary files.
	/// </summary>
	public class GitLogExtended : GitLog
	{
		private readonly static Regex RenameOrCopyExp = new Regex(
			@"(rename|copy) ((?<prefix>.*?){)?(?<old>.*?) => (?<new>.*?)(}(?<sufix>.*?))? \(\d+%\)");

		public GitLogExtended(Stream log,
			IEnumerable<string> parentRevisions,
			IEnumerable<string> childRevisions)
			: base(log, parentRevisions, childRevisions)
		{
		}

		protected override void ParseTouchedFiles(TextReader reader)
		{
			string line;
			touchedFiles = new List<TouchedFile>();

			while ((line = reader.ReadLine()) != null)
			{
				if (line == string.Empty)
				{
					// skip revision info section
					for (int i = 0; i < 7; i++)
					{
						line = reader.ReadLine();
					}
				}

				var blocks = line.Split('	');
				switch (blocks.Length)
				{
					case 3:
						if (blocks[0] != "-" && blocks[1] != "-")
						{
							if (!blocks[2].Contains(" => "))
							{
								AddOrModifyTouchedFile(
									TouchedFileAction.MODIFIED, blocks[2], null);
							}
						}
						break;
					case 1:
						blocks = line.TrimStart(' ').Split(' ');
						var action = GetFileGitAction(blocks[0]);
						switch (action)
						{
							case TouchedFileGitAction.ADDED:
							case TouchedFileGitAction.DELETED:
								MapTouchedFile(action, blocks[3], null, ModifyTouchedFile);
								break;
							case TouchedFileGitAction.RENAMED:
							case TouchedFileGitAction.COPIED:
								var match = RenameOrCopyExp.Match(line);
								if (match.Success)
								{
									string prefix = match.Groups["prefix"].Value;
									string oldPart = match.Groups["old"].Value;
									string newPart = match.Groups["new"].Value;
									string sufix = match.Groups["sufix"].Value;

									var oldPath = (prefix + oldPart + sufix).Replace("//", "/");
									var newPath = (prefix + newPart + sufix).Replace("//", "/");

									AddOrModifyTouchedFile(
										TouchedFileAction.ADDED, newPath, oldPath);
									if (action == TouchedFileGitAction.RENAMED)
									{
										AddOrModifyTouchedFile(
											TouchedFileAction.REMOVED, oldPath, null);
									}
								}
								else
								{
									throw new ArgumentException($"Bad log line {line}.");
								}
								break;
						}
						break;
				}
			}
		}
		protected void ModifyTouchedFile(TouchedFileAction action, string path, string sourcePath)
		{
			path = GitPathToPath(path);
			sourcePath = GitPathToPath(sourcePath);

			var touchedFile = TouchedFiles.SingleOrDefault(x => x.Path == path);
			if (touchedFile != null)
			{
				touchedFile.Action = action;
				touchedFile.SourcePath = sourcePath;
			}
		}
		protected TouchedFileGitAction GetFileGitAction(string action)
		{
			switch (action)
			{
				case "create": return TouchedFileGitAction.ADDED;
				case "delete": return TouchedFileGitAction.DELETED;
				case "rename": return TouchedFileGitAction.RENAMED;
				case "copy": return TouchedFileGitAction.COPIED;
			}
			throw new ApplicationException(string.Format("{0} - is invalid path action", action));
		}
	}
}
