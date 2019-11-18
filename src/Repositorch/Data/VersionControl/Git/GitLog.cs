using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitLog : Log
	{
		public GitLog(Stream log,
			IEnumerable<string> parentRevisions,
			IEnumerable<string> childRevisions)
		{
			TextReader reader = new StreamReader(log);

			Revision = reader.ReadLine();
			AuthorName = reader.ReadLine();
			AuthorEmail = reader.ReadLine();
			Date = DateTime.Parse(reader.ReadLine()).ToUniversalTime();
			Message = reader.ReadLine();
			var tags = reader.ReadLine();
			if (string.IsNullOrEmpty(tags))
			{
				Tags = Enumerable.Empty<string>();
			}
			else
			{
				Regex tagRegExp = new Regex(@"tag: (.*?)(, |$)");
				var matches = tagRegExp.Matches(tags);
				if (matches.Count > 0)
				{
					var list = new List<string>();
					foreach (Match m in matches)
					{
						list.Add(m.Groups[1].Value);
					}
					Tags = list;
				}
			}
			ParentRevisions = parentRevisions;
			ChildRevisions = childRevisions;

			string line;
			string[] blocks;
			TouchedFileGitAction action;
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

				blocks = line.Split('	');
				action = ParsePathAction(blocks[0]);

				switch (action)
				{
					case TouchedFileGitAction.MODIFIED:
						TouchFile(TouchedFileAction.MODIFIED, blocks[1]);
						break;
					case TouchedFileGitAction.ADDED:
						TouchFile(TouchedFileAction.ADDED, blocks[1]);
						break;
					case TouchedFileGitAction.DELETED:
						TouchFile(TouchedFileAction.REMOVED, blocks[1]);
						break;
					case TouchedFileGitAction.RENAMED:
						TouchFile(TouchedFileAction.REMOVED, blocks[1]);
						TouchFile(TouchedFileAction.ADDED, blocks[2], blocks[1]);
						break;
					case TouchedFileGitAction.COPIED:
						TouchFile(TouchedFileAction.ADDED, blocks[2], blocks[1]);
						break;
					default:
						break;
				}
			}
			touchedFiles.Sort((x, y) => string.CompareOrdinal(x.Path.ToLower(), y.Path.ToLower()));
		}
		private void TouchFile(TouchedFileAction action, string path)
		{
			TouchFile(action, path, null);
		}
		private void TouchFile(TouchedFileAction action, string path, string sourcePath)
		{
			path = path.Replace("\"", "");
			if (sourcePath != null)
			{
				sourcePath = sourcePath.Replace("\"", "");
			}

			path = "/" + path;
			if (sourcePath != null)
			{
				sourcePath = "/" + sourcePath;
			}
			var touchedFile = touchedFiles.Where(x => x.Path == path).SingleOrDefault();
			if (touchedFile == null)
			{
				touchedFiles.Add(new TouchedFile()
				{
					Path = path,
					Action = action,
					SourcePath = sourcePath
				});
			}
			else
			{
				if (touchedFile.Action == TouchedFileAction.MODIFIED
					&& action != TouchedFileAction.MODIFIED)
				{
					touchedFile.Action = action;
				}
			}
		}
		private TouchedFileGitAction ParsePathAction(string action)
		{
			switch (action.Substring(0, 1).ToUpper())
			{
				case "M": return TouchedFileGitAction.MODIFIED;
				case "A": return TouchedFileGitAction.ADDED;
				case "D": return TouchedFileGitAction.DELETED;
				case "R": return TouchedFileGitAction.RENAMED;
				case "C": return TouchedFileGitAction.COPIED;
			}
			throw new ApplicationException(string.Format("{0} - is invalid path action", action));
		}
	}
}
