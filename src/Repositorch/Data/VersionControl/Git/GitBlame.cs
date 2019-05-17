using System;
using System.Collections.Generic;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	/// <summary>
	/// Keeps for each line the revision of the last modification.
	/// </summary>
	public class GitBlame : Dictionary<int,string>, IBlame
	{
		public GitBlame(Stream blameData)
		{
			TextReader reader = new StreamReader(blameData);
			
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if ((line.Length >= 46) && (line.Length < 100))
				{
					string[] parts = line.Split(' ');
					if ((parts.Length == 4) && (parts[0].Length == 40))
					{
						int lines = Convert.ToInt32(parts[3]);
						int startLine = Convert.ToInt32(parts[2]);
						for (int i = 0; i < lines; i++)
						{
							Add(startLine + i, parts[0]);
						}
					}
				}
			}
		}
	}
}
