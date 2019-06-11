using System;
using Xunit;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitRevisionsTest
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

		private GitRevisions revList;

		[Fact]
		public void Should_get_all_revisions()
		{
			revList = new GitRevisions(rev_list_1.ToStream());

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
			revList = new GitRevisions(rev_list_1.ToStream());

			Assert.Equal(new string[] { },
				revList.GetRevisionParents("11111"));
			Assert.Equal(new string[] { "11111" },
				revList.GetRevisionParents("22222"));
			Assert.Equal(new string[] { "22222" },
				revList.GetRevisionParents("33333"));
			Assert.Equal(new string[] { "11111" },
				revList.GetRevisionParents("44444"));
			Assert.Equal(new string[] { "44444" },
				revList.GetRevisionParents("55555"));
			Assert.Equal(new string[] { "33333", "55555" },
				revList.GetRevisionParents("66666"));
			Assert.Equal(new string[] { },
				revList.GetRevisionParents("77777"));
			Assert.Equal(new string[] { "66666" },
				revList.GetRevisionParents("88888"));

			Assert.Equal(new string[] { "22222", "44444" },
				revList.GetRevisionChildren("11111"));
			Assert.Equal(new string[] { "33333" },
				revList.GetRevisionChildren("22222"));
			Assert.Equal(new string[] { "66666" },
				revList.GetRevisionChildren("33333"));
			Assert.Equal(new string[] { "55555" },
				revList.GetRevisionChildren("44444"));
			Assert.Equal(new string[] { "66666" },
				revList.GetRevisionChildren("55555"));
			Assert.Equal(new string[] { "88888" },
				revList.GetRevisionChildren("66666"));
			Assert.Equal(new string[] { },
				revList.GetRevisionChildren("77777"));
			Assert.Equal(new string[] { },
				revList.GetRevisionChildren("88888"));
		}
	}
}
