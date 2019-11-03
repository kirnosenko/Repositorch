namespace System.Collections.Generic
{
	public class Cache<K, V>
	{
		private Func<K, V> source;
		protected Dictionary<K, V> data;
		
		public Cache(Func<K, V> source, int initialCacheCapacity = 1000)
		{
			this.source = source;
			this.data = new Dictionary<K, V>(initialCacheCapacity);
		}

		public virtual V GetData(K key)
		{
			if (data.TryGetValue(key, out var value))
			{
				return value;
			}

			value = source(key);
			data.Add(key, value);
			return value;
		}
		public void Clear()
		{
			data.Clear();
		}
	}
}
