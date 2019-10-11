using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Repositorch.Data.VersionControl.Git
{
	/// <summary>
	/// Keeps for each line the revision of the last modification.
	/// </summary>
	public class GitBlame : Dictionary<int,string>, IBlame
	{
		private GitBlame()
		{
		}

		public static GitBlame Parse(Stream blameData)
		{
			var result = new GitBlame();
			result.CheckSum = GetCheckSum(blameData);

			blameData.Seek(0, SeekOrigin.Begin);
			TextReader reader = new StreamReader(blameData);
			string line = reader.ReadLine();
			if (line != null && line.StartsWith("fatal: no such path"))
			{
				return null;
			}

			do
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
							result.Add(startLine + i, parts[0]);
						}
					}
				}
			} while ((line = reader.ReadLine()) != null);

			return result;
		}
		public string CheckSum
		{
			get; set;
		}

		private static string GetCheckSum(Stream stream)
		{
			using (SHA1Managed sha1 = new SHA1Managed())
			{
				byte[] hash = sha1.ComputeHash(stream);
				StringBuilder formatted = new StringBuilder(2 * hash.Length);
				foreach (var b in hash)
				{
					formatted.AppendFormat("{0:x2}", b);
				}
				return formatted.ToString();
			}	
		}
	}
}
