using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace WebApplication1.StructuralPatterns.Proxy
{
    public interface IInventory
    {
        int GetStock(string sku);
        void AddStock(string sku, int qty);
        bool Reserve(string sku, int qty); 

    public sealed class RealInventory : IInventory
    {
        private readonly Dictionary<string, int> _stock = new(StringComparer.OrdinalIgnoreCase);
        private readonly int _latencyMs;
        private int _getStockCalls;
        public int GetStockCalls => _getStockCalls; // để test

        public RealInventory(IDictionary<string, int>? initial = null, int latencyMs = 25)
        {
            if (initial != null)
                foreach (var kv in initial) _stock[kv.Key] = kv.Value;
            _latencyMs = latencyMs;
        }

        public int GetStock(string sku)
        {
            Interlocked.Increment(ref _getStockCalls);
            Thread.Sleep(_latencyMs);
            return _stock.TryGetValue(sku, out var v) ? v : 0;
        }

        public void AddStock(string sku, int qty)
        {
            if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
            Thread.Sleep(_latencyMs);
            _stock[sku] = GetStock(sku) + qty;
        }

        public bool Reserve(string sku, int qty)
        {
            if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
            Thread.Sleep(_latencyMs);
            var cur = GetStock(sku);
            if (cur < qty) return false;
            _stock[sku] = cur - qty;
            return true;
        }
    }

    public sealed class LazyInventoryProxy : IInventory
    {
        private readonly Func<IInventory> _factory;
        private IInventory? _inner;
        public int FactoryCalls { get; private set; }

        public LazyInventoryProxy(Func<IInventory> factory)
            => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        private IInventory Inner
        {
            get
            {
                if (_inner == null)
                {
                    _inner = _factory();
                    FactoryCalls++;
                }
                return _inner;
            }
        }

        public int GetStock(string sku) => Inner.GetStock(sku);
        public void AddStock(string sku, int qty) => Inner.AddStock(sku, qty);
        public bool Reserve(string sku, int qty) => Inner.Reserve(sku, qty);

        public RealInventory? TryGetReal() => _inner as RealInventory;
    }

    //Protection Proxy
    public sealed class ProtectionInventoryProxy : IInventory
    {
        private readonly IInventory _inner;
        private readonly Func<bool> _canWrite;

        public ProtectionInventoryProxy(IInventory inner, Func<bool> canWrite)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _canWrite = canWrite ?? (() => false);
        }

        public int GetStock(string sku) => _inner.GetStock(sku);

        public void AddStock(string sku, int qty)
        {
            if (!_canWrite()) throw new UnauthorizedAccessException("Write access denied");
            _inner.AddStock(sku, qty);
        }

        public bool Reserve(string sku, int qty)
        {
            if (!_canWrite()) throw new UnauthorizedAccessException("Write access denied");
            return _inner.Reserve(sku, qty);
        }
    }

    //Caching Proxy
    public sealed class CachingInventoryProxy : IInventory
    {
        private readonly IInventory _inner;
        private readonly TimeSpan _ttl;
        private readonly ConcurrentDictionary<string, (int value, DateTime expires)> _cache
            = new(StringComparer.OrdinalIgnoreCase);

        public CachingInventoryProxy(IInventory inner, TimeSpan ttl)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _ttl = ttl <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : ttl;
        }

        public int GetStock(string sku)
        {
            if (_cache.TryGetValue(sku, out var entry) && entry.expires > DateTime.UtcNow)
                return entry.value;

            var fresh = _inner.GetStock(sku);
            _cache[sku] = (fresh, DateTime.UtcNow.Add(_ttl));
            return fresh;
        }

        public void AddStock(string sku, int qty)
        {
            _inner.AddStock(sku, qty);
            Invalidate(sku);
        }

        public bool Reserve(string sku, int qty)
        {
            var ok = _inner.Reserve(sku, qty);
            Invalidate(sku);
            return ok;
        }

        private void Invalidate(string sku) => _cache.TryRemove(sku, out _);
    }
}
