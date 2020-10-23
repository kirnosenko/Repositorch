using System;
using System.Collections.Generic;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	/// <summary>
	/// Keeps for each line the revision of the last modification.
	/// </summary>
	public class GitBlame : Dictionary<string, double>, IBlame
	{
		private GitBlame()
		{
		}

		public static GitBlame Parse(Stream blameData)
		{
			if (blameData == null) return null;
			
			TextReader reader = new StreamReader(blameData);
			var result = new GitBlame();
			string line;

			while ((line = reader.ReadLine()) != null)
			{
				if ((line.Length >= 46) && (line.Length < 100))
				{
					string[] parts = line.Split(' ');
					if ((parts.Length == 4) && (parts[0].Length == 40))
					{
						string revision = parts[0];
						int lines = Convert.ToInt32(parts[3]);
						if (!result.ContainsKey(revision))
						{
							result.Add(revision, 0);
						}
						result[revision] += lines;
					}
				}
			}

			return result;
		}
	}
}
