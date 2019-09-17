using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Repositorch.Data.VersionControl.Git
{
	/// <summary>
	/// Keeps list of modified in commit files.
	/// </summary>
	public class GitDiff : List<string>, IDiff
	{
		public GitDiff(Stream diffData)
		{
			TextReader reader = new StreamReader(diffData);

			string prefix = "+++ b";
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.StartsWith(prefix))
				{
					Add(line.Replace(prefix, ""));
				}
			}
		}

		public IEnumerable<string> TouchedFiles
		{
			get { return this; }
		}
	}
}
