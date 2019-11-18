﻿using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public class BaseMapperTest : BaseRepositoryTest
	{
		protected class TestLog : Log
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
			public TestLog FileAdded(string path)
			{
				TouchPath(path, TouchedFileAction.ADDED, null, null);
				return this;
			}
			public TestLog FileModified(string path)
			{
				TouchPath(path, TouchedFileAction.MODIFIED, null, null);
				return this;
			}
			public TestLog FileCopied(string path, string sourcePath, string sourceRevision)
			{
				TouchPath(path, TouchedFileAction.ADDED, sourcePath, sourceRevision);
				return this;
			}
			public TestLog FileRemoved(string path)
			{
				TouchPath(path, TouchedFileAction.REMOVED, null, null);
				return this;
			}
			public TestLog FileRenamed(string path, string sourcePath)
			{
				FileRemoved(sourcePath);
				FileCopied(path, sourcePath, null);
				return this;
			}

			private void TouchPath(string path, TouchedFileAction action, string sourcePath, string sourceRevision)
			{
				touchedFiles.Add(new TouchedFile()
				{
					Path = path,
					Action = action,
					SourcePath = sourcePath,
					SourceRevision = sourceRevision
				});
			}
		}

		protected class TestBlame : Dictionary<int, string>, IBlame
		{
			public TestBlame AddLinesFromRevision(string revision, int lines)
			{
				int from = Count + 1;
				int to = from + lines;
				for (int i = from; i < to; i++)
				{
					this[i] = revision;
				}

				return this;
			}
		}

		protected IVcsData vcsData;
		
		public BaseMapperTest()
		{
			vcsData = Substitute.For<IVcsData>();
		}
	}
}
