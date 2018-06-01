using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if false
namespace DensityBrot
{
	//https://stackoverflow.com/questions/754233/is-it-there-any-lru-implementation-of-idictionary
	public class LRUCache<K,V>
	{
		private int capacity;
		private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
		private LinkedList<LRUCacheItem<K, V>> lruList = new LinkedList<LRUCacheItem<K, V>>();
		private object paddlock = new object();

		public LRUCache(int capacity)
		{
			this.capacity = capacity;
		}

		public V Get(K key)
		{
			V val;
			lock (paddlock)
			{
				if (TryGet(key,out val))
				{
					return val;
				}
			}
			return default(V);
		}

		public bool TryGet(K key, out V val)
		{
			LinkedListNode<LRUCacheItem<K, V>> node;
			lock (paddlock)
			{
				if (cacheMap.TryGetValue(key, out node))
				{
					V value = node.Value.value;
					lruList.Remove(node);
					lruList.AddLast(node);
					val = value;
					return true;
				}
			}
			val = default(V);
			return false;
		}

		public void Add(K key, V val)
		{
			bool isMore = false;
			lock(paddlock)
			{
				isMore = cacheMap.Count >= capacity;
			}
			if (isMore)
			{
				RemoveFirst();
			}

			LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
			LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);

			lock (paddlock)
			{
				lruList.AddLast(node);
				cacheMap.Add(key, node);
			}
		}

		public void AddOrUpdate(K key, V val)
		{
			LinkedListNode<LRUCacheItem<K, V>> node;
			bool isCached = false;
			lock (paddlock)
			{
				if (isCached = cacheMap.TryGetValue(key, out node))
				{
					node.Value.value = val;
					lruList.Remove(node);
					lruList.AddLast(node);
				}
			}
			if (!isCached)
			{
				Add(key,val);
			}
		}

		public delegate void ItemRemovedHandler(object sender, ItemRemovedArgs e);
		public event ItemRemovedHandler ItemRemoved;

		private void RemoveFirst()
		{
			LinkedListNode<LRUCacheItem<K, V>> node = lruList.First;

			// Remove from LRUPriority
			lock(paddlock)
			{
				lruList.RemoveFirst();

				// Remove from cache
				cacheMap.Remove(node.Value.key);
			}

			if (ItemRemoved != null) {
				ItemRemoved(this,new ItemRemovedArgs(node.Value.key,node.Value.value));
			}
		}

		public class ItemRemovedArgs : EventArgs
		{
			public ItemRemovedArgs(K key, V value)
			{
				Key = key;
				Value = value;
			}

			public K Key { get; set; }
			public V Value { get; set; }
		}
	}

	class LRUCacheItem<K,V>
	{
		public LRUCacheItem(K k, V v)
		{
			key = k;
			value = v;
		}
		public K key;
		public V value;
	}
}
#endif