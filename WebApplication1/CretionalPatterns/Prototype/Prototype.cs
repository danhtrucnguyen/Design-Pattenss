using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.CretionalPatterns.Prototype
{
    public interface IDeepCloneable<T>
    {
        T DeepClone();
    }

    public sealed class MediaAsset : IDeepCloneable<MediaAsset>
    {
        public string Url { get; init; }
        public byte[]? Bytes { get; init; }

        public MediaAsset(string url, byte[]? bytes = null)
        {
            Url = url;
            Bytes = bytes;
        }

        public MediaAsset DeepClone()
            => new MediaAsset(Url, Bytes is null ? null : Bytes.ToArray());
    }

    public sealed class ProductTemplate : IDeepCloneable<ProductTemplate>
    {
        public string Sku { get; init; }
        public string Name { get; init; }
        public decimal BasePrice { get; init; }
        public string Category { get; init; }
        public List<string> Tags { get; init; }
        public Dictionary<string, string> Attributes { get; init; }
        public MediaAsset? Media { get; init; }

        public ProductTemplate(
            string sku,
            string name,
            decimal basePrice,
            string category,
            List<string>? tags = null,
            Dictionary<string, string>? attributes = null,
            MediaAsset? media = null)
        {
            Sku = sku;
            Name = name;
            BasePrice = basePrice;
            Category = category;
            Tags = tags ?? new();
            Attributes = attributes ?? new();
            Media = media;
        }

        // Shallow clone (copy tham chiếu)
        public ProductTemplate ShallowClone()
            => (ProductTemplate)MemberwiseClone();

        // Deep clone (copy dữ liệu lồng nhau)
        public ProductTemplate DeepClone()
            => new ProductTemplate(
                sku: Sku,
                name: Name,
                basePrice: BasePrice,
                category: Category,
                tags: new List<string>(Tags),
                attributes: new Dictionary<string, string>(Attributes),
                media: Media?.DeepClone()
            );
    }

    public static class CatalogService
    {
        // Tạo biến thể từ template: clone rồi chỉnh khác biệt
        public static ProductTemplate CreateVariant(ProductTemplate from, string skuSuffix, decimal deltaPrice)
        {
            if (from is null) throw new ArgumentNullException(nameof(from));
            if (string.IsNullOrWhiteSpace(skuSuffix))
                throw new ArgumentException("skuSuffix is required");

            var variant = from.DeepClone();
            variant.Attributes["variant"] = skuSuffix;
            variant.Tags.Add("variant");

            return new ProductTemplate(
                sku: $"{from.Sku}-{skuSuffix}".ToUpperInvariant(),
                name: from.Name,
                basePrice: Math.Max(0, from.BasePrice + deltaPrice),
                category: from.Category,
                tags: variant.Tags,
                attributes: variant.Attributes,
                media: variant.Media
            );
        }
    }
}


//GET http://localhost:5102/api/prototype/demo
//POST http://localhost:5102/api/prototype/clone

/*
[
  {
    "base": {
      "sku": "TSHIRT001",
      "name": "T-Shirt",
      "basePrice": 20,
      "category": "Clothing",
      "tags": ["cotton", "unisex"],
      "attributes": { "size": "M", "color": "white" },
      "media": { "url": "tshirt.jpg", "imageSize": 3 }
    },
    "variants": [
      { "skuSuffix": "RED-L",  "deltaPrice": 5 },
      { "skuSuffix": "BLUE-S", "deltaPrice": 0 }
    ]
  }
]
*/