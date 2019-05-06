﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitData : IVcsData
	{
		private IGitClient git;
		
		public List<string> revisions;
		
		public GitData(IGitClient git)
		{
			this.git = git;
		}
		public string RevisionByNumber(int revisionNumber)
		{
			if (revisions == null)
			{
				GetAllRevisions();
			}
			if (revisionNumber - 1 < revisions.Count)
			{
				return revisions[revisionNumber - 1];
			}
			else
			{
				return null;
			}
		}
		public ILog Log(string revision)
		{
			using (var log = git.Log(revision))
			{
				return new GitLog(log);
			}
		}
		
		private void GetAllRevisions()
		{
			revisions = new List<string>();
			
			using (var revlist = git.RevList())
			{
				TextReader reader = new StreamReader(revlist);
				
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					revisions.Add(line);
				}
			}
		}
	}
}
