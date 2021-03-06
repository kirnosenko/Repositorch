﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositorch.Data.VersionControl
{
	/// <summary>
	/// Revision graph requires adding revisions in topological order.
	/// </summary>
	public class RevisionGraph
	{
		public interface IRevisionNode
		{
			IEnumerable<string> Parents { get; }
			IEnumerable<string> Children { get; }
		}

		class RevisionNode : IRevisionNode
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

		private List<string> revisionsOrdered = new List<string>();
		private Dictionary<string, RevisionNode> revisionsHashed = new Dictionary<string, RevisionNode>();

		public void AddRevision(string revision, params string[] parents)
		{
			revisionsOrdered.Add(revision);
			foreach (var p in parents)
			{
				revisionsHashed[p].AddChild(revision);
			}
			var node = new RevisionNode();
			node.AddParents(parents);
			revisionsHashed.Add(revision, node);
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
		public string GetLastRevision()
		{
			return revisionsOrdered.Last();
		}
		public IRevisionNode GetRevisionNode(string revision)
		{
			return revisionsHashed[revision];
		}
		public IEnumerable<string> GetSplitRevisionsTillRevision(string revisionToStop)
		{
			foreach (var revison in revisionsOrdered)
			{
				if (GetRevisionNode(revison).Children.Count() > 1)
				{
					yield return revison;
					if (revison == revisionToStop)
					{
						yield break;
					}
				}
			}
		}
	}
}
