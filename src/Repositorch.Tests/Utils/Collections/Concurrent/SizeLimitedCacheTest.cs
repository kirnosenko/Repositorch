using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace System.Collections.Concurrent
{
	public class SizeLimitedCacheTest
	{
		private SizeLimitedCache<int, string> cache;
		private ConcurrentQueue<int> sourceCalls;

		public SizeLimitedCacheTest()
		{
			cache = new SizeLimitedCache<int, string>(k =>
			{
				sourceCalls.Enqueue(k);
				return k.ToString();
			}, 5);
			sourceCalls = new ConcurrentQueue<int>();
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
		[Fact]
		public void Should_be_thread_safe()
		{
			var N = 10000;
			var M = N - 100;
			cache = new SizeLimitedCache<int, string>(k =>
			{
				sourceCalls.Enqueue(k);
				return k.ToString();
			}, M);
			var numbers = new List<int>(N);
			for (int i = 0; i < N; i++)
			{
				numbers.Add(i);
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
				.Should().Be(M);
			sourceCalls.Count
				.Should().Be(N);
		}
	}
}
