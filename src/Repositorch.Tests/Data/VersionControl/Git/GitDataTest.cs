using System;
using System.Linq;
using Xunit;
using NSubstitute;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitDataTest
	{
		private IGitClient gitClient;
		private GitData gitData;
		private string nl = Environment.NewLine;
		
		public GitDataTest()
		{
			gitClient = Substitute.For<IGitClient>();
			gitData = new GitData(gitClient);
		}
		[Fact]
		public void Should_return_revision_by_number()
		{
			gitClient
				.RevList()
				.Returns(("a" + nl + "b" + nl + "c").ToStream());

			Assert.Equal("a", gitData.RevisionByNumber(1));
			Assert.Equal("b", gitData.RevisionByNumber(2));
			Assert.Equal("c", gitData.RevisionByNumber(3));
			Assert.Null(gitData.RevisionByNumber(4));
		}
	}
}
