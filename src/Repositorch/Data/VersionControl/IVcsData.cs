using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.VersionControl
{
	public static class VcsDataExtension
	{
		public static bool IsMerge(this IVcsData vcsData, string revision)
		{
			return vcsData.GetRevisionParents(revision).Count() > 1;
		}
		public static bool IsSplit(this IVcsData vcsData, string revision)
		{
			return vcsData.GetRevisionChildren(revision).Count() > 1;
		}
	}

	public interface IVcsData
	{
		string GetRevisionByNumber(int number);
		IEnumerable<string> GetRevisionParents(string revision);
		IEnumerable<string> GetRevisionChildren(string revision);
		
		Log Log(string revision);
		IDiff Diff(string revision);
		IBlame Blame(string revision, string filePath);
	}
}
