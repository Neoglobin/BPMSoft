namespace IntegrationV2.Files.cs.Utils
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading;


	#region Class: HashLocker

	/// <summary>
	/// Provides in-memory locking feature using unique string key for lock.
	/// </summary>
	internal class HashLocker {

		#region Fields: Private

		private readonly ConcurrentDictionary<string, LockItem> _activeLocks = new ConcurrentDictionary<string, LockItem>();

		#endregion

		#region Methods: Public

		/// <summary>
		/// Creates or returns existing lock object for <paramref name="key"/>
		/// </summary>
		/// <param name="key">Unique string.</param>
		/// <returns><see cref="LockItem"/> instance to lock on.</returns>
		public IDisposable Lock(string key) {
			var activeLock = _activeLocks.GetOrAdd(key, xKey => new LockItem(_activeLocks, xKey));
			activeLock.Enter();
			return activeLock;
		}

		#endregion

	}

	#endregion

	#region Class: LockItem

	/// <summary>
	/// Lock item for in-memory locking.
	/// </summary>
	internal class LockItem : IDisposable {

		#region Fields: Private

		private readonly ConcurrentDictionary<string, LockItem> _items;
		private readonly string _key;
		private readonly object _locker = new object();
		private volatile int _lockCount;

		#endregion

		#region Constructors: internal

		/// <summary>
		/// .ctor
		/// </summary>
		/// <param name="items">Lock items collection.</param>
		/// <param name="key">String key to lock on.</param>
		internal LockItem(ConcurrentDictionary<string, LockItem> items, string key) {
			_items = items;
			_key = key;
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Locks on current item.
		/// </summary>
		public void Enter() {
			Interlocked.Increment(ref _lockCount);
			Monitor.Enter(_locker);
		}

		/// <summary>
		/// Unlocks current item on dispose.
		/// </summary>
		public void Dispose() {
			Monitor.PulseAll(_locker);
			Monitor.Exit(_locker);
			if (Interlocked.Decrement(ref _lockCount) == 0) {
				_items.TryRemove(_key, out var item);
			}
		}

		#endregion

	}

	#endregion

}
