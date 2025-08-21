using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.Observer
{
    //DTOs
    public sealed class ProductDto { public string Sku { get; set; } = ""; public decimal Price { get; set; } public int Stock { get; set; } }
    public sealed class WatcherDto  // "price" | "stock"
    {
        public string Type { get; set; } = "price";
        public string Name { get; set; } = "Watcher";
        public decimal? TargetPrice { get; set; } // cho price
        public int? Threshold { get; set; }       // cho stock
    }
    public sealed class ActionDto  // "price" | "stock"
    {
        public string Type { get; set; } = "price";
        public decimal? NewValue { get; set; }
    }
    public sealed class ObserverJobDto
    {
        public string? Title { get; set; }
        public ProductDto Product { get; set; } = new();
        public List<WatcherDto> Watchers { get; set; } = new();
        public List<ActionDto> Actions { get; set; } = new();
    }

    //Runner
    public static class ObserverExample
    {
        public static object Run(List<ObserverJobDto> jobs)
        {
            var outputs = new List<object>();

            foreach (var j in jobs ?? new())
            {
                try
                {
                    var p = new ProductSubject(j.Product.Sku, j.Product.Price, j.Product.Stock);

                    foreach (var w in j.Watchers ?? new())
                    {
                        switch ((w.Type ?? "").Trim().ToLowerInvariant())
                        {
                            case "price":
                                p.Attach(new PriceDropObserver(w.Name ?? "Watcher", w.TargetPrice ?? j.Product.Price));
                                break;
                            case "stock":
                                p.Attach(new LowStockObserver(w.Name ?? "Watcher", w.Threshold ?? 1));
                                break;
                            default: break;
                        }
                    }

                    var steps = new List<object>();

                    foreach (var a in j.Actions ?? new())
                    {
                        List<string> notes = new();
                        var t = (a.Type ?? "").Trim().ToLowerInvariant();
                        if (t == "price")
                            notes = new List<string>(p.SetPrice(a.NewValue ?? p.Price));
                        else if (t == "stock")
                            notes = new List<string>(p.SetStock((int)(a.NewValue ?? p.Stock)));

                        steps.Add(new
                        {
                            action = t,
                            newValue = a.NewValue,
                            state = new { sku = p.Sku, price = p.Price, stock = p.Stock },
                            notifications = notes
                        });
                    }

                    outputs.Add(new
                    {
                        title = j.Title,
                        initial = new { j.Product.Sku, j.Product.Price, j.Product.Stock },
                        steps
                    });
                }
                catch (Exception ex)
                {
                    outputs.Add(new { title = j.Title, error = ex.Message });
                }
            }

            return new { count = outputs.Count, results = outputs };
        }
    }
}
