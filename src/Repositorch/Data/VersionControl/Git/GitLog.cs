﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Repositorch.Data.VersionControl.Git
{
	/// <summary>
	/// Simple version of git log to get information about commit 
	/// and touched files.
	/// </summary>
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

			ParseTouchedFiles(reader);
		}

		protected virtual void ParseTouchedFiles(TextReader reader)
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
				var action = ParsePathAction(blocks[0]);
				string path = blocks[1];
				string sourcePath = null;
				if (action == TouchedFileGitAction.RENAMED || action == TouchedFileGitAction.COPIED)
				{
					path = blocks[2];
					sourcePath = blocks[1];
				}
				MapTouchedFile(action, path, sourcePath, AddOrModifyTouchedFile);
			}
			touchedFiles.Sort((x, y) => string.CompareOrdinal(x.Path.ToLower(), y.Path.ToLower()));
		}
		protected void MapTouchedFile(
			TouchedFileGitAction action,
			string path,
			string sourcePath,
			Action<TouchedFileAction,string,string> touchedFileMapper)
		{
			switch (action)
			{
				case TouchedFileGitAction.MODIFIED:
					touchedFileMapper(TouchedFileAction.MODIFIED, path, sourcePath);
					break;
				case TouchedFileGitAction.ADDED:
					touchedFileMapper(TouchedFileAction.ADDED, path, sourcePath);
					break;
				case TouchedFileGitAction.DELETED:
					touchedFileMapper(TouchedFileAction.REMOVED, path, sourcePath);
					break;
				case TouchedFileGitAction.RENAMED:
					touchedFileMapper(TouchedFileAction.REMOVED, sourcePath, null);
					touchedFileMapper(TouchedFileAction.ADDED, path, sourcePath);
					break;
				case TouchedFileGitAction.COPIED:
					touchedFileMapper(TouchedFileAction.ADDED, path, sourcePath);
					break;
				default:
					break;
			}
		}
		protected string GitPathToPath(string path)
		{
			if (path == null)
			{
				return null;
			}

			return "/" + path.Replace("\"", "");
		}
		protected void AddOrModifyTouchedFile(TouchedFileAction action, string path, string sourcePath)
		{
			path = GitPathToPath(path);
			sourcePath = GitPathToPath(sourcePath);

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
