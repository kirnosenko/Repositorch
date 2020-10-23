using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.VersionControl
{
	public class TestLog : Log
	{
		public TestLog(string revision)
			: this(revision, "", "", DateTime.Now, "")
		{
		}
		public TestLog(string revision, string name, string email, DateTime date, string message)
		{
			Revision = revision;
			AuthorName = name;
			AuthorEmail = email;
			Date = date;
			Message = message;
			Tags = Enumerable.Empty<string>();
			ParentRevisions = Enumerable.Empty<string>();
			ChildRevisions = Enumerable.Empty<string>();
			touchedFiles = new List<TouchedFile>();
		}

		public TestLog TagsAre(params string[] tags)
		{
			Tags = tags;
			return this;
		}
		public TestLog ParentRevisionsAre(params string[] parentRevisions)
		{
			ParentRevisions = parentRevisions;
			return this;
		}
		public TestLog ChildRevisionsAre(params string[] childRevisions)
		{
			ChildRevisions = childRevisions;
			return this;
		}
		public TestLog FileAdded(
			string path,
			TouchedFile.ContentType type = TouchedFile.ContentType.UNKNOWN)
		{
			TouchPath(path, TouchedFileAction.ADDED, null, null, type);
			return this;
		}
		public TestLog FileModified(
			string path,
			TouchedFile.ContentType type = TouchedFile.ContentType.UNKNOWN)
		{
			TouchPath(path, TouchedFileAction.MODIFIED, null, null, type);
			return this;
		}
		public TestLog FileCopied(
			string path,
			string sourcePath,
			string sourceRevision,
			TouchedFile.ContentType type = TouchedFile.ContentType.UNKNOWN)
		{
			TouchPath(path, TouchedFileAction.ADDED, sourcePath, sourceRevision, type);
			return this;
		}
		public TestLog FileRemoved(
			string path,
			TouchedFile.ContentType type = TouchedFile.ContentType.UNKNOWN)
		{
			TouchPath(path, TouchedFileAction.REMOVED, null, null, type);
			return this;
		}
		public TestLog FileRenamed(
			string path,
			string sourcePath,
			TouchedFile.ContentType type = TouchedFile.ContentType.UNKNOWN)
		{
			FileRemoved(sourcePath, type);
			FileCopied(path, sourcePath, null, type);
			return this;
		}

		private void TouchPath(
			string path,
			TouchedFileAction action,
			string sourcePath,
			string sourceRevision,
			TouchedFile.ContentType type)
		{
			touchedFiles.Add(new TouchedFile()
			{
				Path = path,
				Action = action,
				SourcePath = sourcePath,
				SourceRevision = sourceRevision,
				Type = type,
			});
		}
	}
}
