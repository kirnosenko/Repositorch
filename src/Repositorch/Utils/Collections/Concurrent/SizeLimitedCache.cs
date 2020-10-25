namespace System.Collections.Concurrent
{
	public class SizeLimitedCache<K, V> : Cache<K, V>
	{
		private int sizeLimit;
		private ConcurrentQueue<K> keys;

		public SizeLimitedCache(Func<K, V> source, int sizeLimit)
			: base(source, sizeLimit + 1)
		{
			this.sizeLimit = sizeLimit;
			this.keys = new ConcurrentQueue<K>();
		}

		public override V GetData(K key)
		{
			int preSize = data.Count;
			var value = base.GetData(key);

			if (preSize < data.Count)
			{
				keys.Enqueue(key);
				while (data.Count > sizeLimit)
				{
					if (keys.TryDequeue(out var keyToRemove))
					{
						data.TryRemove(keyToRemove, out _);
					}
				}
			}
			
			return value;
		}
	}
}
