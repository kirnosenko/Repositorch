﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitLog : Log
	{
		public GitLog(Stream log)
		{
			TextReader reader = new StreamReader(log);

			Revision = reader.ReadLine();
			Author = reader.ReadLine();
			Date = DateTime.Parse(reader.ReadLine()).ToUniversalTime();
			Message = reader.ReadLine();

			string line;
			string[] blocks;
			TouchedFileGitAction action;
			touchedFiles = new List<TouchedFile>();

			while ((line = reader.ReadLine()) != null)
			{
				blocks = line.Split('	');
				action = ParsePathAction(blocks[0]);

				switch (action)
				{
					case TouchedFileGitAction.MODIFIED:
						TouchFile(TouchedFile.TouchedFileAction.MODIFIED, blocks[1]);
						break;
					case TouchedFileGitAction.ADDED:
						TouchFile(TouchedFile.TouchedFileAction.ADDED, blocks[1]);
						break;
					case TouchedFileGitAction.DELETED:
						TouchFile(TouchedFile.TouchedFileAction.REMOVED, blocks[1]);
						break;
					case TouchedFileGitAction.RENAMED:
						TouchFile(TouchedFile.TouchedFileAction.REMOVED, blocks[1]);
						TouchFile(TouchedFile.TouchedFileAction.ADDED, blocks[2], blocks[1]);
						break;
					case TouchedFileGitAction.COPIED:
						TouchFile(TouchedFile.TouchedFileAction.ADDED, blocks[2], blocks[1]);
						break;
					default:
						break;
				}
			}
			touchedFiles.Sort((x, y) => string.CompareOrdinal(x.Path.ToLower(), y.Path.ToLower()));
		}
		private void TouchFile(TouchedFile.TouchedFileAction action, string path)
		{
			TouchFile(action, path, null);
		}
		private void TouchFile(TouchedFile.TouchedFileAction action, string path, string sourcePath)
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
			touchedFiles.Add(new TouchedFile()
			{
				Path = path,
				Action = action,
				SourcePath = sourcePath
			});
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
