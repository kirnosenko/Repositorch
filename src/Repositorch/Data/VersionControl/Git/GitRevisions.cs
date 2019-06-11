using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitRevisions
	{
		class RevisionNode
		{
			private LinkedList<string> parents;
			private LinkedList<string> children;

			public RevisionNode()
			{
				parents = new LinkedList<string>();
				children = new LinkedList<string>();
			}
			public void AddParents(IEnumerable<string> parents)
			{
				foreach (var p in parents)
				{
					this.parents.AddLast(p);
				}
			}
			public void AddChild(string child)
			{
				children.AddLast(child);
			}
			public IEnumerable<string> Parents
			{
				get { return parents; }
			}
			public IEnumerable<string> Children
			{
				get { return children; }
			}
		}

		private static readonly char[] lineSeparator = new char[] { ' ' };
		private List<string> revisionsOrdered = new List<string>();
		private Dictionary<string, RevisionNode> revisionsHashed = new Dictionary<string, RevisionNode>();

		public GitRevisions(Stream revList)
		{
			TextReader reader = new StreamReader(revList);
			
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				var lineRevisions = line.Split(lineSeparator);
				var revision = lineRevisions.First();
				revisionsOrdered.Add(revision);
				var parents = lineRevisions.Skip(1);
				foreach (var p in parents)
				{
					revisionsHashed[p].AddChild(revision);
				}
				var node = new RevisionNode();
				node.AddParents(parents);
				revisionsHashed.Add(revision, node);
			}
		}
		public int RevisionCount
		{
			get { return revisionsOrdered.Count; }
		}
		public string GetRevisionByNumber(int number)
		{
			var index = number - 1;
			if (index < 0 || index >= revisionsOrdered.Count)
			{
				return null;
			}
			return revisionsOrdered[index];
		}
		public IEnumerable<string> GetRevisionParents(string revision)
		{
			return revisionsHashed[revision].Parents;
		}
		public IEnumerable<string> GetRevisionChildren(string revision)
		{
			return revisionsHashed[revision].Children;
		}
	}
}
