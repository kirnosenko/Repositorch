using System;
using Xunit;
using NSubstitute;

namespace Repositorch.Data.VersionControl
{
	public class VcsDataCachedTest
	{
		private IVcsData data;
		private VcsDataCached dataCached;

		public VcsDataCachedTest()
		{
			data = Substitute.For<IVcsData>();
			dataCached = new VcsDataCached(data, 5);
		}
		[Fact]
		public void Should_cache_data_and_use_them_later()
		{
			dataCached.Log("1");
			dataCached.Log("1");
			dataCached.Log("2");
			dataCached.Log("2");
			dataCached.Log("3");

			data.Received(1).Log(Arg.Is<string>("1"));
			data.Received(1).Log(Arg.Is<string>("2"));
			data.Received(1).Log(Arg.Is<string>("3"));
			data.Received(3).Log(Arg.Any<string>());
		}
		[Fact]
		public void Should_not_keep_data_beyond_size_limit()
		{
			dataCached.Log("1");
			dataCached.Log("2");
			dataCached.Log("3");
			dataCached.Log("4");
			dataCached.Log("5");
			dataCached.Log("6");
			dataCached.Log("1");

			data.Received(2).Log(Arg.Is<string>("1"));
		}
	}
}
