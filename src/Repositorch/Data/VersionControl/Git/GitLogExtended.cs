using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Repositorch.Data.VersionControl.Git
{
	/// <summary>
	/// Extended version of git log.
	/// Allows to identify and ignore git-detected
	/// binary files, symbolic links, submodules.
	/// Use this if you are not sure.
	/// </summary>
	public class GitLogExtended : GitLog
	{
		private static readonly Regex AddOrDeleteExp = new Regex(
			@"(create mode|delete mode) (?<mode>\d{6}) (?<fullpath>.*)$");
		private static readonly Regex RenameOrCopyExp = new Regex(
			@"(rename|copy) (?<fullpath>(((?<prefix>.*?){)?(?<old>.*?) => (?<new>.*?)(}(?<sufix>.*?))?)) \(\d+%\)");

		public GitLogExtended(Stream log,
			IEnumerable<string> parentRevisions,
			IEnumerable<string> childRevisions)
			: base(log, parentRevisions, childRevisions)
		{
		}

		protected override void ParseTouchedFiles(TextReader reader, int infoLines)
		{
			string line;
			touchedFiles = new List<TouchedFile>();
			List<string> binaryPaths = new List<string>();

			while ((line = reader.ReadLine()) != null)
			{
				if (line == string.Empty)
				{
					// skip revision info section
					for (int i = 0; i < infoLines + 1; i++)
					{
						line = reader.ReadLine();
					}
				}

				var blocks = line.Split('	');
				switch (blocks.Length)
				{
					case 3:
						{
							var isBinary = (blocks[0] == "-" && blocks[1] == "-");
							var path = blocks[2];
							if (isBinary)
							{
								binaryPaths.Add(path);
							}
							if (!path.Contains(" => "))
							{
								AddOrModifyTouchedFile(
									TouchedFileAction.MODIFIED,
									path,
									null,
									isBinary
										? TouchedFile.ContentType.BINARY
										: TouchedFile.ContentType.TEXT);
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
								var addOrDeleteMatch = AddOrDeleteExp.Match(line);
								if (addOrDeleteMatch.Success)
								{
									string fullpath = addOrDeleteMatch.Groups["fullpath"].Value;
									string mode = addOrDeleteMatch.Groups["mode"].Value;

									if (!mode.EndsWith("0000"))
									{
										MapTouchedFile(
											action,
											fullpath,
											null,
											binaryPaths.Contains(fullpath)
												? TouchedFile.ContentType.BINARY
												: TouchedFile.ContentType.TEXT,
											ModifyTouchedFile);
									}
									else
									{
										// this is a symbolic link or gitlink
										RemoveTouchedFile(fullpath);
									}
								}
								else
								{
									throw new ArgumentException($"Bad log line {line}.");
								}
								break;
							case TouchedFileGitAction.RENAMED:
							case TouchedFileGitAction.COPIED:
								var renameOrCopyMatch = RenameOrCopyExp.Match(line);
								if (renameOrCopyMatch.Success)
								{
									string fullpath = renameOrCopyMatch.Groups["fullpath"].Value;
									string prefix = renameOrCopyMatch.Groups["prefix"].Value;
									string oldPart = renameOrCopyMatch.Groups["old"].Value;
									string newPart = renameOrCopyMatch.Groups["new"].Value;
									string sufix = renameOrCopyMatch.Groups["sufix"].Value;

									var oldPath = (prefix + oldPart + sufix).Replace("//", "/");
									var newPath = (prefix + newPart + sufix).Replace("//", "/");

									var isBinary = binaryPaths.Contains(fullpath);
									AddOrModifyTouchedFile(
										TouchedFileAction.ADDED,
										newPath,
										oldPath,
										isBinary
											? TouchedFile.ContentType.BINARY
											: TouchedFile.ContentType.TEXT);
									if (action == TouchedFileGitAction.RENAMED)
									{
										AddOrModifyTouchedFile(
											TouchedFileAction.REMOVED,
											oldPath,
											null,
											isBinary
												? TouchedFile.ContentType.BINARY
												: TouchedFile.ContentType.TEXT);
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
		protected void ModifyTouchedFile(
			TouchedFileAction action,
			string path,
			string sourcePath,
			TouchedFile.ContentType type)
		{
			path = GitPathToPath(path);
			sourcePath = GitPathToPath(sourcePath);

			var touchedFile = touchedFiles.SingleOrDefault(x => x.Path == path);
			if (touchedFile != null)
			{
				touchedFile.Action = action;
				touchedFile.SourcePath = sourcePath;
				touchedFile.Type = type;
			}
		}
		protected void RemoveTouchedFile(string path)
		{
			path = GitPathToPath(path);
			
			var touchedFile = touchedFiles.Single(x => x.Path == path);
			touchedFiles.Remove(touchedFile);
		}
		protected TouchedFileGitAction GetFileGitAction(string action)
		{
			switch (action)
			{
				case "create": return TouchedFileGitAction.ADDED;
				case "delete": return TouchedFileGitAction.DELETED;
				case "rename": return TouchedFileGitAction.RENAMED;
				case "copy": return TouchedFileGitAction.COPIED;
				case "mode": return TouchedFileGitAction.MODIFIED;
			}
			throw new ApplicationException(string.Format("{0} - is invalid path action", action));
		}
	}
}
