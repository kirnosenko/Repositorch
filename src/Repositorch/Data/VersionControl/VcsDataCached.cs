﻿using System;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public class VcsDataCached : IVcsData
	{
		private IVcsData innerData;
		private Cache<string, Log> logs;
		private Cache<(string,string), IBlame> blames;

		public VcsDataCached(IVcsData innerData, int logsLimit, int blamesLimit)
		{
			this.innerData = innerData;
			this.logs = new Cache<string, Log>(logsLimit, innerData.Log);
			this.blames = new Cache<(string revision, string path), IBlame>(
				blamesLimit, k => innerData.Blame(k.revision, k.path));
		}

		public string RevisionByNumber(int revisionNumber)
		{
			return innerData.RevisionByNumber(revisionNumber);
		}
		public string[] ParentRevisions(string revision)
		{
			return innerData.ParentRevisions(revision);
		}
		public Log Log(string revision)
		{
			return logs.GetData(revision);
		}
		public IBlame Blame(string revision, string filePath)
		{
			return blames.GetData((revision,filePath));
		}
	}
}
