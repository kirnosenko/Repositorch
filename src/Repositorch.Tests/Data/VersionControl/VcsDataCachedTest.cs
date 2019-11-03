using System;
using Xunit;
using NSubstitute;

namespace Repositorch.Data.VersionControl
{
	public class VcsDataCachedTest
	{
		private IVcsData innerData;
		private IVcsData cache;

		public VcsDataCachedTest()
		{
			innerData = Substitute.For<IVcsData>();
			cache = new VcsDataCached(innerData);
		}

		[Fact]
		public void Should_cache_logs()
		{
			innerData.Log("1")
				.Returns(Substitute.For<Log>());

			var log1 = cache.Log("1");
			var log2 = cache.Log("1");

			Assert.True(log1 == log2);
			innerData.Received(1)
				.Log(Arg.Is<string>(x => x == "1"));
			innerData.DidNotReceive()
				.Log(Arg.Is<string>(x => x != "1"));
		}
		[Fact]
		public void Should_cache_blames_for_curent_revision()
		{
			innerData.Blame("1", "file1")
				.Returns(c => Substitute.For<IBlame>());
			innerData.Blame("1", "file2")
				.Returns(c => Substitute.For<IBlame>());
			innerData.Blame("2", "file1")
				.Returns(c => Substitute.For<IBlame>());
			innerData.Blame("2", "file2")
				.Returns(c => Substitute.For<IBlame>());

			var blame1 = cache.Blame("1", "file1");
			var blame2 = cache.Blame("1", "file2");
			var blame3 = cache.Blame("1", "file1");

			Assert.False(blame1 == blame2);
			Assert.True(blame1 == blame3);

			var blame4 = cache.Blame("2", "file1");
			var blame5 = cache.Blame("2", "file2");
			var blame6 = cache.Blame("2", "file2");

			Assert.False(blame4 == blame5);
			Assert.True(blame5 == blame6);

			var blame7 = cache.Blame("1", "file1");

			Assert.False(blame1 == blame7);

			innerData.Received(2)
				.Blame(Arg.Is<string>(x => x == "1"), Arg.Is<string>(x => x == "file1"));
			innerData.Received(1)
				.Blame(Arg.Is<string>(x => x == "2"), Arg.Is<string>(x => x == "file2"));
		}
	}
}
