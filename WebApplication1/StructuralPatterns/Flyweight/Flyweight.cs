using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace WebApplication1.StructuralPatterns.Flyweight
{
    // Intrinsic state
    public sealed class ProductShared
    {
        public string Sku { get; }
        public string Name { get; }
        public string Brand { get; }
        public byte[] ImageBytes { get; } 
        public IReadOnlyDictionary<string, string> Attributes { get; }

        public ProductShared(string sku, string name, string brand, byte[] imageBytes,
            IDictionary<string, string>? attributes = null)
        {
            Sku = sku ?? throw new ArgumentNullException(nameof(sku));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Brand = brand ?? throw new ArgumentNullException(nameof(brand));
            ImageBytes = imageBytes ?? throw new ArgumentNullException(nameof(imageBytes));
            Attributes = new Dictionary<string, string>(attributes ?? new Dictionary<string, string>());
        }
    }

    // Extrinsic state
    public sealed record ProductViewContext(
        string StoreId, decimal Price, int Stock, string Variant, int GridX, int GridY
    );

    // Flyweight
    public interface IProductFlyweight
    {
        string Sku { get; }
        string Render(ProductViewContext ctx); // demo: trả về chuỗi render
    }

    public sealed class ProductFlyweight : IProductFlyweight
    {
        private readonly ProductShared _shared;
        public string Sku => _shared.Sku;

        public ProductFlyweight(ProductShared shared)
            => _shared = shared ?? throw new ArgumentNullException(nameof(shared));

        public string Render(ProductViewContext ctx)
        {
            return $"[{ctx.GridX},{ctx.GridY}] {_shared.Brand} {_shared.Name} ({_shared.Sku}) " +
                   $"Variant={ctx.Variant} Price={ctx.Price} Stock={ctx.Stock} Store={ctx.StoreId} " +
                   $"ImgBytes={_shared.ImageBytes.Length}";
        }
    }

    // Factory
    public sealed class FlyweightFactory
    {
        private readonly ConcurrentDictionary<string, Lazy<IProductFlyweight>> _cache =
            new(StringComparer.OrdinalIgnoreCase);

        public IProductFlyweight GetOrCreate(string sku, Func<string, ProductShared> loader)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU is required");
            if (loader is null) throw new ArgumentNullException(nameof(loader));

            var lazy = _cache.GetOrAdd(
                sku,
                key => new Lazy<IProductFlyweight>(
                    () => new ProductFlyweight(loader(key)),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            return lazy.Value;
        }

        public int Count => _cache.Count;

        public bool TryGet(string sku, out IProductFlyweight? fw)
        {
            fw = null;
            if (_cache.TryGetValue(sku, out var lazy) && lazy.IsValueCreated)
            {
                fw = lazy.Value;
                return true;
            }
            return false;
        }
    }
}
