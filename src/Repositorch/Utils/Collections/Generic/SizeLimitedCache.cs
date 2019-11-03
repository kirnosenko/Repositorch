namespace System.Collections.Generic
{
	public class SizeLimitedCache<K, V> : Cache<K, V>
	{
		private int sizeLimit;
		private Queue<K> keys;

		public SizeLimitedCache(Func<K, V> source, int sizeLimit)
			: base(source, sizeLimit + 1)
		{
			this.sizeLimit = sizeLimit;
			this.keys = new Queue<K>(sizeLimit + 1);
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
					var keyToRemove = keys.Dequeue();
					data.Remove(keyToRemove);
				}
			}
			
			return value;
		}
	}
}
