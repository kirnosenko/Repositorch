using Xunit;

namespace System.Collections.Generic
{
	public class SizeLimitedCacheTest
	{
		private SizeLimitedCache<int, string> cache;
		private List<int> sourceCalls;

		public SizeLimitedCacheTest()
		{
			cache = new SizeLimitedCache<int, string>(k =>
			{
				sourceCalls.Add(k);
				return k.ToString();
			}, 5);
			sourceCalls = new List<int>();
		}
		[Fact]
		public void Should_cache_data_and_use_them_later()
		{
			cache.GetData(1);
			cache.GetData(1);
			cache.GetData(2);
			cache.GetData(2);
			cache.GetData(3);

			Assert.Equal(new int[] { 1, 2, 3 }, sourceCalls);
		}
		[Fact]
		public void Should_not_keep_data_beyond_size_limit()
		{
			cache.GetData(1);
			cache.GetData(2);
			cache.GetData(3);
			cache.GetData(4);
			cache.GetData(5);
			cache.GetData(6);
			cache.GetData(1);

			Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 1 }, sourceCalls);
		}
	}
}
