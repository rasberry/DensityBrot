using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

// = find or create lock for key
// = if created - store lock in dictionary
// = aquire lock using object or block
// = release lock
// = if no one is using lock - remove from collection

namespace DensityBrot
{
	#if false
	// TODO turns out this doesn't really work 100% - also makes things *very* slow
	//Notes: (Works!!)
	// - It seems to be that to use wait() you have to signal all threads on Set()
	// and that is why ManualResetEvent works and not AutoResetEvent or SemaphoreSlim(1,1)
	// - Looks like the reason this works is because TryAdd can happen between
	// TryRemove and Set() which can cause the 'release' to happen on the wrong 'sync object'
	public class PebbleLock<T> : IDisposable
	{
		static ConcurrentDictionary<T,ManualResetEvent> store
			= new ConcurrentDictionary<T, ManualResetEvent>();

		public PebbleLock(T key)
		{
			Key = key;
			while (!store.TryAdd(key,new ManualResetEvent(false))) {
				if(store.TryGetValue(key, out var e)) {
					e.WaitOne();
				}
			}
		}

		public void Dispose()
		{
			if (!store.TryRemove(Key,out var e)) {
				throw new InvalidOperationException("key removal failed");
			}
			e.Set();
			wasDisposed = true;
		}

		~PebbleLock()
		{
			if (!wasDisposed) {
				throw new InvalidOperationException("Dispose was never called. Consider a employing a 'using' statement");
			}
		}

		T Key;
		bool wasDisposed = false;
	}
	#endif

	#if false
	//Notes: (Works!!)
	// - not sure which to use Thread.Sleep or Thread.Yield
	// - simle design but will call tryadd a lot under high contention
	// - much slower than the version that uses ManualResetEvent
	// https://codereview.stackexchange.com/questions/128599/dynamic-multi-threading-lock-with-while-loop
	public class PebbleLock<T> : IDisposable
	{
		static ConcurrentDictionary<T,byte> store
			= new ConcurrentDictionary<T, byte>();

		public PebbleLock(T key)
		{
			this.Key = key;
			while (!store.TryAdd(key,0)) {
				//TODO this causes many TryAdd attempts if the key is highly contended
				Thread.Sleep(0);
			}
		}

		public void Dispose()
		{
			if (!store.TryRemove(Key,out _)) {
				throw new InvalidOperationException("key removal failed");
			}
			wasDisposed = true;
		}

		~PebbleLock()
		{
			if (!wasDisposed) {
				throw new InvalidOperationException("Dispose was never called. Consider a employing a using statement");
			}
		}

		T Key;
		bool wasDisposed = false;
	}
	#endif

	//#if false
	//
	public class PebbleLock<T> : IDisposable
	{
		static ConcurrentDictionary<T,int> store
			= new ConcurrentDictionary<T,int>();

		public PebbleLock(T item)
		{
			Key = item;
			while(!store.TryAdd(item,Thread.CurrentThread.ManagedThreadId)) {
				Thread.Sleep(1); //TODO can this be a ResetEvent ?
			}
		}

		public void Dispose()
		{
			if (!store.TryRemove(Key,out int threadid)) {
				throw new InvalidOperationException("failed to remove key");
			} else {
				if (threadid != Thread.CurrentThread.ManagedThreadId) {
					throw new InvalidOperationException("thread id's don't match");
				}
			}
		}

		T Key;
	}
	//#endif

	#if false
	//Notes: (works!!)
	//TODO trying different approach where we keep track of just {thread count} number of items
	// then check if another thread is using the same key. Seems like a sound idea, but this
	// - also seem fairly slow tho
	public class PebbleLock<T> : IDisposable
	{
		static Dictionary<T,int> store = new Dictionary<T, int>();
		static object locker = new object();

		public PebbleLock(T item)
		{
			Key = item;
			bool wasAquired = false;
			while(!wasAquired)
			{
				lock(locker)
				{
					if (!store.TryGetValue(item,out int thread)) {
						store.Add(item,Thread.CurrentThread.ManagedThreadId);
						wasAquired = true;
					}
				}
				if (!wasAquired) {
					Thread.Sleep(1); //TODO can this be a ResetEvent ?
				}
			}
		}

		public void Dispose()
		{
			lock(locker)
			{
				if (!store.Remove(Key,out int thread)) {
					throw new InvalidOperationException("failed to remove key");
				} else {
					if (thread != Thread.CurrentThread.ManagedThreadId) {
						throw new InvalidOperationException("thread id's don't match");
					}
				}
			}
		}

		T Key;
	}
	#endif

	#if false
	//TODO this one still calls TryAdd repeatedly.. not sure why tho
	public class PebbleLock<T> : IDisposable
	{
		static ConcurrentDictionary<T,AutoResetEvent> store
			= new ConcurrentDictionary<T, AutoResetEvent>();

		public PebbleLock(T key)
		{
			Key = key;
			while (!store.TryAdd(key,Are = new AutoResetEvent(false))) {
				Console.WriteLine("\ttryadd "+System.Threading.Thread.CurrentThread.ManagedThreadId);
				if(Are == null && store.TryGetValue(key, out Are)) {
					Console.WriteLine("\twait "+System.Threading.Thread.CurrentThread.ManagedThreadId);
					Are.WaitOne();
				}
			}
		}

		public void Dispose()
		{
			if (!store.TryRemove(Key,out _)) {
				throw new InvalidOperationException("key removal failed");
			}
			Console.WriteLine("\trelease "+System.Threading.Thread.CurrentThread.ManagedThreadId);
			Are.Set();
		}

		T Key;
		AutoResetEvent Are;
	}
	#endif

	#if false
	//TODO not sure why using SemaphoreSlim is any different than ManualResetEvent, but
	// there must be a difference since the SemaphoreSlim version hangs
	public class PebbleLock<T> : IDisposable
	{
		static ConcurrentDictionary<T,SemaphoreSlim> store
			= new ConcurrentDictionary<T, SemaphoreSlim>();

		public PebbleLock(T key)
		{
			Key = key;
			while (!store.TryAdd(key,new SemaphoreSlim(1,1))) {
				//Console.WriteLine("\ttryadd "+System.Threading.Thread.CurrentThread.ManagedThreadId);
				if(store.TryGetValue(key, out var e)) {
					//Console.WriteLine("\twait "+System.Threading.Thread.CurrentThread.ManagedThreadId);
					e.Wait();
				}
			}
		}

		public void Dispose()
		{
			if (!store.TryRemove(Key,out var e)) {
				throw new InvalidOperationException("key removal failed");
			}
			if (e.CurrentCount > 0) {
				//Console.WriteLine("\trelease "+System.Threading.Thread.CurrentThread.ManagedThreadId);
				e.Release();
			}
		}

		T Key;
	}
	#endif

	#if false
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
			var weakHandle = store.GetOrAdd(key,(T k) => {
				lockHandle = new object();
				return new WeakReference(lockHandle);
			});
			//TODO ohnoes.. lockHandle might be null here if it was GC'd during disuse
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
			GC.RegisterForFullGCNotification(1,1);
			Console.WriteLine("sweep registered");
			while(true) { //TODO replace while loop with something that stops when the main thread stops - http://localhost:8080/stackoverflow/7402146
				GC.WaitForFullGCComplete(); //blocks
				Console.WriteLine("GC happened");
				foreach(var kvp in store) {
					if (!kvp.Value.IsAlive) {
						if (store.TryRemove(kvp.Key,out _)) {
							Console.WriteLine("removed "+kvp.Key);
						}
					}
				}
			}
		}

		object lockHandle;
		bool wasTaken;
	}
	#endif

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
	//TODO you need a peblelock to implement counting.. badness
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