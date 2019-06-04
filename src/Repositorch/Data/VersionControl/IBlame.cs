using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.VersionControl
{
	public static class BlameExtension
	{
		public static IDictionary<string, int> Diff(this IBlame blame1, IBlame blame2)
		{
			var lineByRevision1 = from l in blame1 group l.Key by l.Value;
			var lineByRevision2 = from l in blame2 group l.Key by l.Value;
			var revisions = lineByRevision1.Select(r1 => r1.Key).Union(lineByRevision2.Select(r2 => r2.Key));

			var result = new Dictionary<string, int>();
			foreach (var diff in
				from r in revisions
				join r1 in lineByRevision1 on r equals r1.Key into r1g
				join r2 in lineByRevision2 on r equals r2.Key into r2g
				from rl1 in r1g.DefaultIfEmpty()
				from rl2 in r2g.DefaultIfEmpty()
				select new
				{
					Revision = r,
					Delta = (rl2 == null ? 0 : rl2.Count()) - (rl1 == null ? 0 : rl1.Count()),
				})
			{
				if (diff.Delta != 0)
				{
					result.Add(diff.Revision, diff.Delta);
				}
			}
			return result;
		}
	}

	public interface IBlame : IDictionary<int, string>
	{
	}
}
