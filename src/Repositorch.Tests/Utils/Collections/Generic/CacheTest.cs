using Xunit;

namespace System.Collections.Generic
{
	public class CacheTest
	{
		private Cache<int, string> cache;
		private List<int> sourceCalls;

		public CacheTest()
		{
			cache = new Cache<int, string>(k =>
			{
				sourceCalls.Add(k);
				return k.ToString();
			});
			sourceCalls = new List<int>();
		}
		[Fact]
		public void Should_cache_data_and_use_them_later()
		{
			Assert.Equal("1", cache.GetData(1));
			Assert.Equal("1", cache.GetData(1));
			Assert.Equal("2", cache.GetData(2));
			Assert.Equal("2", cache.GetData(2));
			Assert.Equal("3", cache.GetData(3));
			Assert.Equal("1", cache.GetData(1));

			Assert.Equal(new int[] { 1, 2, 3 }, sourceCalls);
		}
		[Fact]
		public void Should_clear_cache()
		{
			Assert.Equal("1", cache.GetData(1));
			Assert.Equal("1", cache.GetData(1));
			cache.Clear();
			Assert.Equal("1", cache.GetData(1));
			Assert.Equal("1", cache.GetData(1));

			Assert.Equal(new int[] { 1, 1 }, sourceCalls);
		}
	}
}
