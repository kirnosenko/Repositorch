namespace System.Collections.Concurrent
{
	public class Cache<K, V>
	{
		private Func<K, V> source;
		protected ConcurrentDictionary<K, V> data;
		
		public Cache(Func<K, V> source, int initialCacheCapacity = 1000)
		{
			this.source = source;
			this.data = new ConcurrentDictionary<K, V>(16, initialCacheCapacity);
		}

		public virtual V GetData(K key)
		{
			if (data.TryGetValue(key, out var value))
			{
				return value;
			}

			value = source(key);
			data.TryAdd(key, value);
			return value;
		}
		public void Clear()
		{
			data.Clear();
		}
		public int Count
		{
			get { return data.Count; }
		}
	}
}
