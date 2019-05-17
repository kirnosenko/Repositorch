namespace System.Collections.Generic
{
	public class Cache<K, V>
	{
		private int sizeLimit;
		private Func<K, V> source;
		private Dictionary<K, V> data;
		private Queue<K> keys;

		public Cache(int sizeLimit, Func<K, V> source)
		{
			this.sizeLimit = sizeLimit;
			this.source = source;
			this.data = new Dictionary<K, V>(sizeLimit);
			this.keys = new Queue<K>(sizeLimit);
		}
		public V GetData(K key)
		{
			if (data.ContainsKey(key))
			{
				return data[key];
			}
			while (data.Count >= sizeLimit)
			{
				var keyToRemove = keys.Dequeue();
				data.Remove(keyToRemove);
			}
			V value = source(key);
			data.Add(key, value);
			keys.Enqueue(key);
			return value;
		}
	}
}
