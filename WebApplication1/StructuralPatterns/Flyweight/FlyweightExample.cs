using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.StructuralPatterns.Flyweight
{
    // ===== DTOs =====
    public sealed class ProductDefDto
    {
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public int ImageSize { get; set; } = 1024; // bytes length giả lập
        public Dictionary<string, string>? Attributes { get; set; }
    }

    public sealed class FlyweightJobDto
    {
        public string Sku { get; set; } = "";
        public string StoreId { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Variant { get; set; } = "Default";
        public int GridX { get; set; }
        public int GridY { get; set; }

        // Tuỳ chọn: shared data cho lần xuất hiện đầu của SKU
        public ProductDefDto? Product { get; set; }
    }

    public static class FlyweightExample
    {
        public static object Run(List<FlyweightJobDto> jobs)
        {
            if (jobs == null) throw new ArgumentNullException(nameof(jobs));

            // gom shared data từ các job có cung cấp Product
            var provided = jobs
                .Where(j => j.Product != null && !string.IsNullOrWhiteSpace(j.Sku))
                .GroupBy(j => j.Sku, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var p = g.First().Product!;
                        var size = p.ImageSize <= 0 ? 1024 : p.ImageSize;
                        return new
                        {
                            p.Name,
                            p.Brand,
                            Image = NewBytes(size),
                            Attr = (IReadOnlyDictionary<string, string>?)(p.Attributes ?? new Dictionary<string, string>())
                        };
                    },
                    StringComparer.OrdinalIgnoreCase
                );

            var factory = new FlyweightFactory();

            ProductShared Loader(string sku)
            {
                // nếu có dữ liệu cung cấp sẵn thì dùng, nếu không tự sinh mặc định
                if (provided.TryGetValue(sku, out var def))
                    return new ProductShared(sku, def.Name, def.Brand, def.Image, def.Attr?.ToDictionary(kv => kv.Key, kv => kv.Value));

                var img = NewBytes(1024);
                return new ProductShared(
                    sku,
                    name: $"Product {sku}",
                    brand: "BrandX",
                    imageBytes: img,
                    attributes: new Dictionary<string, string> { ["Color"] = "Red", ["Size"] = "M" }
                );
            }

            var results = new List<object>();

            foreach (var j in jobs)
            {
                if (string.IsNullOrWhiteSpace(j.Sku))
                {
                    results.Add(new { error = "SKU is required" });
                    continue;
                }

                // kiểm tra cache hit trước khi tạo
                bool cacheHitBefore = factory.TryGet(j.Sku, out _);

                var fw = factory.GetOrCreate(j.Sku, Loader);

                var ctx = new ProductViewContext(
                    StoreId: j.StoreId,
                    Price: j.Price,
                    Stock: j.Stock,
                    Variant: j.Variant,
                    GridX: j.GridX,
                    GridY: j.GridY
                );

                var rendered = fw.Render(ctx);

                results.Add(new
                {
                    j.Sku,
                    cacheHit = cacheHitBefore,
                    output = rendered
                });
            }

            return new
            {
                flyweightsCreated = factory.Count,
                uniqueSkus = jobs.Select(x => x.Sku).Where(s => !string.IsNullOrWhiteSpace(s))
                                 .Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                results
            };
        }

        private static byte[] NewBytes(int size)
        {
            var b = new byte[size];
            new Random().NextBytes(b);
            return b;
        }
    }
}
