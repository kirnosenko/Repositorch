using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.VersionControl
{
	public static class BlameExtension
	{
		public static IDictionary<string, double> Diff(this IBlame blame1, IBlame blame2)
		{
			var revisions = blame1.Keys.Union(blame2.Keys);
			var diff = revisions.Select(x => new
			{
				Revision = x,
				Delta = (blame2.ContainsKey(x) ? blame2[x] : 0) - 
						(blame1.ContainsKey(x) ? blame1[x] : 0)
			}).ToArray();

			return diff
				.Where(x => x.Delta != 0)
				.ToDictionary(x => x.Revision, x => x.Delta);
		}
	}

	public interface IBlame : IReadOnlyDictionary<string, double>
	{
	}
}
