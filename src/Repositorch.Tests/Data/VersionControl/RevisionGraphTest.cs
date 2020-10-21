using System;
using Xunit;
using FluentAssertions;

namespace Repositorch.Data.VersionControl
{
	public class RevisionGraphTest
	{
		private RevisionGraph graph;
		
		public RevisionGraphTest()
		{
			graph = new RevisionGraph();
		}

		[Fact]
		public void Should_keep_graph_data()
		{
			graph.AddRevision("1");
			graph.AddRevision("11", "1");
			graph.AddRevision("101", "1");
			graph.AddRevision("111", "11", "101");

			graph.RevisionCount
				.Should().Be(4);
			graph.GetRevisionByNumber(1)
				.Should().Be("1");
			graph.GetRevisionByNumber(2)
				.Should().Be("11");
			graph.GetRevisionByNumber(3)
				.Should().Be("101");
			graph.GetRevisionByNumber(4)
				.Should().Be("111");
			graph.GetRevisionByNumber(5)
				.Should().BeNull();
			graph.GetRevisionNode("1").Parents
				.Should().BeEmpty();
			graph.GetRevisionNode("1").Children
				.Should().BeEquivalentTo(new string[] { "11", "101" });
			graph.GetRevisionNode("11").Parents
				.Should().BeEquivalentTo(new string[] { "1" });
			graph.GetRevisionNode("11").Children
				.Should().BeEquivalentTo(new string[] { "111" });
			graph.GetRevisionNode("101").Parents
				.Should().BeEquivalentTo(new string[] { "1" });
			graph.GetRevisionNode("101").Children
				.Should().BeEquivalentTo(new string[] { "111" });
			graph.GetRevisionNode("111").Parents
				.Should().BeEquivalentTo(new string[] { "11", "101" });
			graph.GetRevisionNode("111").Children
				.Should().BeEmpty();
		}
	}
}
