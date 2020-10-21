using System;
using Xunit;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitRevisionGraphTest
	{

private string rev_list_1 =
@"11111
22222 11111
33333 22222
44444 11111
55555 44444
66666 33333 55555
77777
88888 66666";

		private GitRevisionGraph revList;

		[Fact]
		public void Should_get_all_revisions()
		{
			revList = new GitRevisionGraph(rev_list_1.ToStream());

			Assert.Equal(8, revList.RevisionCount);
			for (int i = 1; i <= 8; i++)
			{
				var expected = new String(Convert.ToChar(i.ToString()), 5);
				Assert.Equal(expected, revList.GetRevisionByNumber(i));
			}
			Assert.Null(revList.GetRevisionByNumber(9));
		}
		[Fact]
		public void Should_create_revisions_graph()
		{
			revList = new GitRevisionGraph(rev_list_1.ToStream());

			Assert.Equal(new string[] { },
				revList.GetRevisionNode("11111").Parents);
			Assert.Equal(new string[] { "11111" },
				revList.GetRevisionNode("22222").Parents);
			Assert.Equal(new string[] { "22222" },
				revList.GetRevisionNode("33333").Parents);
			Assert.Equal(new string[] { "11111" },
				revList.GetRevisionNode("44444").Parents);
			Assert.Equal(new string[] { "44444" },
				revList.GetRevisionNode("55555").Parents);
			Assert.Equal(new string[] { "33333", "55555" },
				revList.GetRevisionNode("66666").Parents);
			Assert.Equal(new string[] { },
				revList.GetRevisionNode("77777").Parents);
			Assert.Equal(new string[] { "66666" },
				revList.GetRevisionNode("88888").Parents);

			Assert.Equal(new string[] { "22222", "44444" },
				revList.GetRevisionNode("11111").Children);
			Assert.Equal(new string[] { "33333" },
				revList.GetRevisionNode("22222").Children);
			Assert.Equal(new string[] { "66666" },
				revList.GetRevisionNode("33333").Children);
			Assert.Equal(new string[] { "55555" },
				revList.GetRevisionNode("44444").Children);
			Assert.Equal(new string[] { "66666" },
				revList.GetRevisionNode("55555").Children);
			Assert.Equal(new string[] { "88888" },
				revList.GetRevisionNode("66666").Children);
			Assert.Equal(new string[] { },
				revList.GetRevisionNode("77777").Children);
			Assert.Equal(new string[] { },
				revList.GetRevisionNode("88888").Children);
		}
	}
}
