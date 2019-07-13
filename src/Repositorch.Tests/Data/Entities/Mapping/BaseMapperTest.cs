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
			public TestLog(string revision, string author, DateTime date, string message)
			{
				Revision = revision;
				Author = author;
				Date = date;
				Message = message;
				touchedFiles = new List<TouchedFile>();
			}

			public void FileAdded(string path)
			{
				TouchPath(path, TouchedFile.TouchedFileAction.ADDED, null, null);
			}
			public void FileModified(string path)
			{
				TouchPath(path, TouchedFile.TouchedFileAction.MODIFIED, null, null);
			}
			public void FileCopied(string path, string sourcePath, string sourceRevision)
			{
				TouchPath(path, TouchedFile.TouchedFileAction.ADDED, sourcePath, sourceRevision);
			}
			public void FileRemoved(string path)
			{
				TouchPath(path, TouchedFile.TouchedFileAction.REMOVED, null, null);
			}
			public void FileRenamed(string path, string sourcePath)
			{
				FileRemoved(sourcePath);
				FileCopied(path, sourcePath, null);
			}

			private void TouchPath(string path, TouchedFile.TouchedFileAction action, string sourcePath, string sourceRevision)
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

		protected IVcsData vcsData;
		
		public BaseMapperTest()
		{
			vcsData = Substitute.For<IVcsData>();
		}
	}
}
