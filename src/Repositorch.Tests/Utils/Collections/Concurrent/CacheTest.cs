using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace System.Collections.Concurrent
{
	public class CacheTest
	{
		private Cache<int, string> cache;
		private ConcurrentQueue<int> sourceCalls;

		public CacheTest()
		{
			cache = new Cache<int, string>(k =>
			{
				sourceCalls.Enqueue(k);
				return k.ToString();
			});
			sourceCalls = new ConcurrentQueue<int>();
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
		[Fact]
		public void Should_be_thread_safe()
		{
			var N = 10000;
			var numbers = new ConcurrentQueue<int>();
			for (int i = 0; i < N; i++)
			{
				numbers.Enqueue(i);
			}

			Parallel.ForEach(
				numbers,
				new ParallelOptions
				{
					MaxDegreeOfParallelism = 16
				},
				number =>
				{
					cache.GetData(number)
						.Should().Be(number.ToString());
				});

			cache.Count
				.Should().Be(N);
			sourceCalls.Count
				.Should().Be(N);
		}
	}
}
