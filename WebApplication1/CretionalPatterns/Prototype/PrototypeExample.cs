using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.CretionalPatterns.Prototype
{
    // ===== DTOs cho Postman =====
    public sealed class MediaDto
    {
        public string Url { get; set; } = "";
        // Kích thước bytes giả lập (tránh gửi base64), nếu null sẽ để null
        public int? ImageSize { get; set; }
    }

    public sealed class ProductDto
    {
        public string Sku { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal BasePrice { get; set; }
        public string Category { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, string> Attributes { get; set; } = new();
        public MediaDto? Media { get; set; }
    }

    public sealed class VariantRequestDto
    {
        public string SkuSuffix { get; set; } = "";
        public decimal DeltaPrice { get; set; } = 0m;
    }

    public sealed class PrototypeJobDto
    {
        public ProductDto Base { get; set; } = new();
        public List<VariantRequestDto> Variants { get; set; } = new();
    }

    public static class PrototypeExample
    {
        public static object Run(List<PrototypeJobDto> jobs)
        {
            var results = new List<object>();

            foreach (var j in jobs)
            {
                try
                {
                    // Map base -> domain template
                    var baseMedia = j.Base.Media is null
                        ? null
                        : new MediaAsset(
                            j.Base.Media.Url,
                            j.Base.Media.ImageSize is int n && n > 0 ? RandomBytes(n) : null
                          );

                    var baseTemplate = new ProductTemplate(
                        sku: j.Base.Sku,
                        name: j.Base.Name,
                        basePrice: j.Base.BasePrice,
                        category: j.Base.Category,
                        tags: j.Base.Tags?.ToList() ?? new List<string>(),
                        attributes: j.Base.Attributes?.ToDictionary(kv => kv.Key, kv => kv.Value)
                                   ?? new Dictionary<string, string>(),
                        media: baseMedia
                    );

                    // Tạo các biến thể
                    var variants = new List<object>();
                    foreach (var v in j.Variants ?? new List<VariantRequestDto>())
                    {
                        var p = CatalogService.CreateVariant(baseTemplate, v.SkuSuffix, v.DeltaPrice);
                        variants.Add(ToDtoView(p));
                    }

                    results.Add(new
                    {
                        @base = ToDtoView(baseTemplate),
                        variants
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new { error = ex.Message });
                }
            }

            return new { count = results.Count, results };
        }

        private static object ToDtoView(ProductTemplate p) => new
        {
            sku = p.Sku,
            name = p.Name,
            basePrice = p.BasePrice,
            category = p.Category,
            tags = p.Tags,
            attributes = p.Attributes,
            media = p.Media is null ? null : new { url = p.Media.Url, bytes = p.Media.Bytes?.Length }
        };

        private static byte[] RandomBytes(int len)
        {
            var b = new byte[len];
            new Random().NextBytes(b);
            return b;
        }
    }
}
