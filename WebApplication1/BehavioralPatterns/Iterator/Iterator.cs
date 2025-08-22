using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Iterator
{
    //Domain
    public sealed record Product(string Sku, string Name, string Category, decimal Price, int Stock, decimal Rating);

    public sealed class ProductFilter
    {
        public HashSet<string> Categories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public decimal? MaxPrice { get; init; }
        public bool InStockOnly { get; init; }
        public decimal? MinRating { get; init; }

        public bool Matches(Product p)
        {
            if (Categories.Count > 0 && !Categories.Contains(p.Category)) return false;
            if (MaxPrice is not null && p.Price > MaxPrice.Value) return false;
            if (InStockOnly && p.Stock <= 0) return false;
            if (MinRating is not null && p.Rating < MinRating.Value) return false;
            return true;
        }
    }

    //Iterator contracts
    public interface IProductIterator
    {
        bool HasNext();
        Product Next();     
        void Reset();
    }

    public interface IProductAggregate
    {
        IProductIterator CreateIterator(ProductFilter filter);
    }

    //Aggregate + Iterator
    public sealed class ProductCatalogAggregate : IProductAggregate
    {
        private readonly List<Product> _items;
        public ProductCatalogAggregate(IEnumerable<Product> items)
            => _items = (items ?? Array.Empty<Product>()).ToList();

        public IProductIterator CreateIterator(ProductFilter filter)
            => new FilteredProductIterator(_items, filter ?? new ProductFilter());
    }

    internal sealed class FilteredProductIterator : IProductIterator
    {
        private readonly List<Product> _items;
        private readonly ProductFilter _filter;
        private int _index = -1; 
        public FilteredProductIterator(List<Product> items, ProductFilter filter)
        { _items = items; _filter = filter; }

        public void Reset() => _index = -1;

        public bool HasNext()
        {
            for (int i = _index + 1; i < _items.Count; i++)
                if (_filter.Matches(_items[i])) return true;
            return false;
        }

        public Product Next()
        {
            for (int i = _index + 1; i < _items.Count; i++)
            {
                if (_filter.Matches(_items[i]))
                {
                    _index = i;
                    return _items[i];
                }
            }
            throw new InvalidOperationException("No more items");
        }
    }
}

//GET http://localhost:5102/api/iterator/demo
//POST http://localhost:5102/api/iterator/run

/*
[
  {
    "title": "Accessory ≤ 100, in-stock, rating ≥ 4, pageSize=2",
    "pageSize": 2,
    "filter": { "categories": ["accessory"], "maxPrice": 100, "inStockOnly": true, "minRating": 4 },
    "products": [
      { "sku": "MS-01",  "name": "Mouse",        "category": "accessory", "price": 25,  "stock": 10, "rating": 4.2 },
      { "sku": "KB-01",  "name": "Keyboard",     "category": "accessory", "price": 45,  "stock": 0,  "rating": 4.0 },
      { "sku": "HD-1TB", "name": "SSD 1TB",      "category": "accessory", "price": 95,  "stock": 7,  "rating": 3.9 },
      { "sku": "MON-27", "name": "Monitor 27",   "category": "accessory", "price": 210, "stock": 2,  "rating": 4.6 },
      { "sku": "CAB-01", "name": "USB-C Cable",  "category": "accessory", "price": 9,   "stock": 100,"rating": 4.8 }
    ]
  }
]
*/