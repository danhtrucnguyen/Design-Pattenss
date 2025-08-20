using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Iterator
{
    //DTOs
    public sealed class ProductDto
    {
        public string Sku { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public decimal Rating { get; set; }
    }

    public sealed class FilterDto
    {
        public List<string>? Categories { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStockOnly { get; set; }
        public decimal? MinRating { get; set; }
    }

    public sealed class IteratorJobDto
    {
        public string? Title { get; set; }
        public int? PageSize { get; set; } = 5;
        public FilterDto? Filter { get; set; }
        public List<ProductDto> Products { get; set; } = new();
    }

    //Client run
    public static class IteratorExample
    {
        public static object Run(List<IteratorJobDto> jobs)
        {
            var outs = new List<object>();

            foreach (var j in jobs ?? new())
            {
                try
                {
                    var catalog = new ProductCatalogAggregate(
                        (j.Products ?? new()).Select(p => new Product(
                            p.Sku, p.Name, p.Category, p.Price, p.Stock, p.Rating
                        ))
                    );

                    var filter = new ProductFilter
                    {
                        MaxPrice = j.Filter?.MaxPrice,
                        InStockOnly = j.Filter?.InStockOnly ?? false,
                        MinRating = j.Filter?.MinRating
                    };
                    foreach (var c in j.Filter?.Categories ?? new List<string>())
                        filter.Categories.Add(c);

                    var it = catalog.CreateIterator(filter);
                    var pageSize = Math.Max(1, j.PageSize ?? 5);

                    var pages = new List<object>();
                    int visited = 0;

                    while (it.HasNext())
                    {
                        var page = new List<object>();
                        for (int i = 0; i < pageSize && it.HasNext(); i++)
                        {
                            var x = it.Next(); visited++;
                            page.Add(new { x.Sku, x.Name, x.Category, x.Price, x.Stock, x.Rating });
                        }
                        pages.Add(new { count = page.Count, items = page });
                    }

                    outs.Add(new
                    {
                        title = j.Title,
                        pageSize = pageSize,
                        visited = visited,
                        pages
                    });
                }
                catch (Exception ex)
                {
                    outs.Add(new { title = j?.Title, error = ex.Message });
                }
            }

            return new { count = outs.Count, results = outs };
        }
    }
}
