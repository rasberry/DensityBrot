using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

// = find or create lock for key
// = if created - store lock in dictionary
// = aquire lock using object or block
// = release lock
// = if no one is using lock - remove from collection

namespace DensityBrot
{
	#if false
	//TODO when do you remove the item from the dictionary ?
	public class PebleLock<T> : IDisposable
	{
		static ConcurrentDictionary<T,SemaphoreSlim> store
			 = new ConcurrentDictionary<T, SemaphoreSlim>();

		public PebleLock(T item)
		{
			key = item;
			sema = store.GetOrAdd(key,(T key) => new SemaphoreSlim(1,1));
			sema.Wait();
		}

		public void Dispose()
		{
			sema.Release();
		}

		T key;
		SemaphoreSlim sema;
	}
	#endif

	#if false
	//TODO you need a peblelock to implement counting.. lame
	public class PebleLock_2<T> : IDisposable
	{
		static ConcurrentDictionary<T,object> store = new ConcurrentDictionary<T,object>();
		static ConcurrentDictionary<T,int> count = new ConcurrentDictionary<T, int>();

		public PebleLock_2(T item)
		{
			key = item;
			object o = store.GetOrAdd(item,new object());
			Monitor.Enter(o,ref wasTaken);
			if (wasTaken) {
				count.AddOrUpdate(item,0,(T key,int val) => val+1);
			}
		}

		public void Dispose()
		{
			if (wasTaken) {
				if (!count.TryGetValue(key,out int c)) {
					throw new Exception("Badness!");
				}
				if (c <= 0) {
					//store.TryRemove(key,
				}
				store.TryGetValue(key,out object o);
				Monitor.Exit(o);
			}
		}

		//Tracker Inc(T item,Tracker existing) {
		//	return new Tracker(existing.Count+1, existing.LockObject);
		//}
		//Tracker Dec(T item,Tracker existing) {
		//	return new Tracker(existing.Count-1, existing.LockObject);
		//}

		~PebleLock_2()
		{
			//TODO crash here if dispose was never called
		}

		bool wasTaken;
		T key;

		//struct Tracker
		//{
		//	public Tracker(int count,object lockObject)
		//	{
		//		Count = count;
		//		LockObject = lockObject;
		//	}
		//	public int Count;
		//	public object LockObject;
//
		//	public static Tracker New()
		//	{
		//		return new Tracker(0,new Object());
		//	}
		//}
	}
	#endif

	public class PebbleLock<T> : IDisposable
	{
		static ConcurrentDictionary<T,WeakReference> store;

		static PebbleLock()
		{
			store = new ConcurrentDictionary<T, WeakReference>();
			Thread cleaner = new Thread(Sweep);
			cleaner.Start();
		}

		public PebbleLock(T key)
		{
			var weakHandle = store.GetOrAdd(key,(T k) => new WeakReference(new object()));
			//tie the lifetime of lockHandle to this class so it doesn't get GC'd early
			lockHandle = weakHandle.Target;
			//aquire lock (blocks here)
			Monitor.Enter(lockHandle,ref wasTaken);
		}

		public void Dispose()
		{
			//release lock
			if (wasTaken) {
				Monitor.Exit(lockHandle);
			}
			lockHandle = null;
		}

		~PebbleLock()
		{
			//TODO if lockHandle is not null something very bad happened and we should crash
			// Maybe Dispose was not called by accident :)
		}

		static void Sweep()
		{
			GC.RegisterForFullGCNotification(0,0);
			while(true) { //TODO replace while loop with something that stops when the main thread stops - http://localhost:8080/stackoverflow/7402146
				GC.WaitForFullGCComplete(); //blocks
				foreach(var kvp in store) {
					if (!kvp.Value.IsAlive) {
						store.TryRemove(kvp.Key,out _);
					}
				}
			}
		}

		object lockHandle;
		bool wasTaken;
	}

	#if false
	//TODO find a better way to do the sweeping
	public static class PebleLock_1<T>
	{
		static ConcurrentDictionary<T,WeakReference> store;

		public static object FindLockObject(T index)
		{
			WeakReference r = store.GetOrAdd(index,new WeakReference(new object()));
			Sweep();
			return r.Target;
		}

		static int count = 0;
		const int sweepInterval = 128;
		static void Sweep()
		{
			Interlocked.Increment(ref count);
			if (count > sweepInterval) {
				foreach(var kvp in store) {
					if (!kvp.Value.IsAlive) {
						store.TryRemove(kvp.Key,out _);
					}
				}
			}
		}
	}
	#endif
}