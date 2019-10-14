using System;
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
			public TestLog(string revision, string name, string email, DateTime date, string message)
			{
				Revision = revision;
				AuthorName = name;
				AuthorEmail = email;
				Date = date;
				Message = message;
				touchedFiles = new List<TouchedFile>();
			}

			public void FileAdded(string path)
			{
				TouchPath(path, TouchedFileAction.ADDED, null, null);
			}
			public void FileModified(string path)
			{
				TouchPath(path, TouchedFileAction.MODIFIED, null, null);
			}
			public void FileCopied(string path, string sourcePath, string sourceRevision)
			{
				TouchPath(path, TouchedFileAction.ADDED, sourcePath, sourceRevision);
			}
			public void FileRemoved(string path)
			{
				TouchPath(path, TouchedFileAction.REMOVED, null, null);
			}
			public void FileRenamed(string path, string sourcePath)
			{
				FileRemoved(sourcePath);
				FileCopied(path, sourcePath, null);
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
