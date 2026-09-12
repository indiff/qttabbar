using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QTTabBarLib
{
    /// <summary>
    /// 线程安全的固定容量 LRU 缓存
    /// 适用于 Shell Extension 等长期运行、不可引入重型依赖的场景
    /// </summary>
    internal sealed class LruCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _map;
        private readonly LinkedList<CacheEntry> _list;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private struct CacheEntry
        {
            public TKey Key;
            public TValue Value;
        }

        public LruCache(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _map = new Dictionary<TKey, LinkedListNode<CacheEntry>>(capacity);
            _list = new LinkedList<CacheEntry>();
        }

        /// <summary>
        /// 检查键是否存在（不更新访问顺序，不加写锁）
        /// 仅用于存在性判断，如需读取值请使用 TryGet
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                return _map.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_map.TryGetValue(key, out var node))
                {
                    // 命中：提升到链表头部（MRU端）
                    _lock.EnterWriteLock();
                    try
                    {
                        _list.Remove(node);
                        _list.AddFirst(node);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    value = node.Value.Value;
                    return true;
                }
                value = default;
                return false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// 索引器：get 等价于 TryGet（未命中返回 default），set 等价于 Set
        /// ⚠️ get 未命中时返回 default(TValue) 而非抛异常，与 Dictionary 行为不同
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                TryGet(key, out TValue value);
                return value;
            }
            set => Set(key, value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_map.TryGetValue(key, out var node))
                {
                    // 命中：提升到链表头部（MRU端）
                    _lock.EnterWriteLock();
                    try
                    {
                        _list.Remove(node);
                        _list.AddFirst(node);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    value = node.Value.Value;
                    return true;
                }
                value = default;
                return false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }


        public void Set(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_map.TryGetValue(key, out var existing))
                {
                    // 更新已有条目并提升到头部
                    existing.Value = new CacheEntry { Key = key, Value = value };
                    _list.Remove(existing);
                    _list.AddFirst(existing);
                }
                else
                {
                    // 新增条目
                    if (_map.Count >= _capacity)
                    {
                        // 淘汰尾部（LRU端）
                        var tail = _list.Last;
                        _map.Remove(tail.Value.Key);
                        _list.RemoveLast();
                    }
                    var newNode = new LinkedListNode<CacheEntry>(
                        new CacheEntry { Key = key, Value = value });
                    _list.AddFirst(newNode);
                    _map[key] = newNode;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 清空缓存（用于会话重置或手动清理）
        /// </summary>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _map.Clear();
                _list.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try { return _map.Count; }
                finally { _lock.ExitReadLock(); }
            }
        }
    }
}
