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
								var mode = blocks[2];
								var path = blocks[3];
								if (!mode.EndsWith("0000"))
								{
									MapTouchedFile(
										action,
										path,
										null,
										binaryPaths.Contains(path)
											? TouchedFile.ContentType.BINARY
											: TouchedFile.ContentType.TEXT,
										ModifyTouchedFile);
								}
								else
								{
									// this is not a symbolic link or gitlink
									RemoveTouchedFile(path);
								}
								break;
							case TouchedFileGitAction.RENAMED:
							case TouchedFileGitAction.COPIED:
								var match = RenameOrCopyExp.Match(line);
								if (match.Success)
								{
									string fullpath = match.Groups["fullpath"].Value;
									string prefix = match.Groups["prefix"].Value;
									string oldPart = match.Groups["old"].Value;
									string newPart = match.Groups["new"].Value;
									string sufix = match.Groups["sufix"].Value;

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
