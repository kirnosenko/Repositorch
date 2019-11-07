using System;
using System.Collections.Generic;

namespace Repositorch.Data.VersionControl
{
	public class VcsDataCached : IVcsData
	{
		private IVcsData innerData;
		private Cache<string, Log> logs;
		private string blamesRevision;
		private Cache<(string,string), IBlame> blames;

		public VcsDataCached(
			IVcsData innerData,
			int logsCacheSizeLimit = 1000,
			int blamesCacheInitialCapacity = 1000)
		{
			this.innerData = innerData;
			logs = new SizeLimitedCache<string, Log>(
				innerData.Log,
				logsCacheSizeLimit);
			blamesRevision = null;
			blames = new Cache<(string revision, string path), IBlame>(
				k => innerData.Blame(k.revision, k.path),
				blamesCacheInitialCapacity);
		}

		public string GetRevisionByNumber(int number)
		{
			return innerData.GetRevisionByNumber(number);
		}
		
		public Log Log(string revision)
		{
			return logs.GetData(revision);
		}
		public IBlame Blame(string revision, string filePath)
		{
			if (revision != blamesRevision)
			{
				blames.Clear();
				blamesRevision = revision;
			}

			return blames.GetData((revision, filePath));
		}
	}
}
