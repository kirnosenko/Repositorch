using System;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public class VcsDataCached : IVcsData
	{
		private IVcsData innerData;
		private Cache<string, Log> logs;
		private Cache<(string,string), IBlame> blames;

		public VcsDataCached(IVcsData innerData, int revisionsLimit, int filesLimit)
		{
			this.innerData = innerData;
			this.logs = new Cache<string, Log>(revisionsLimit, innerData.Log);
			this.blames = new Cache<(string revision, string path), IBlame>(
				filesLimit, k => innerData.Blame(k.revision, k.path));
		}

		public string GetRevisionByNumber(int number)
		{
			return innerData.GetRevisionByNumber(number);
		}
		public IEnumerable<string> GetRevisionParents(string revision)
		{
			return innerData.GetRevisionParents(revision);
		}
		public IEnumerable<string> GetRevisionChildren(string revision)
		{
			return innerData.GetRevisionChildren(revision);
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
