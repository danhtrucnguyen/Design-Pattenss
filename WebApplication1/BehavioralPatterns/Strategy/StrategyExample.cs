using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Strategy
{
    // ===== DTOs =====
    public sealed class ItemDto { public string Sku { get; set; } = ""; public int Qty { get; set; } public decimal UnitPrice { get; set; } }
    public sealed class StrategyDto
    {
        // "none" | "percent" | "fixed" | "bulk"
        public string Type { get; set; } = "none";
        public decimal? Percent { get; set; }
        public decimal? Amount { get; set; }
        public int? MinQty { get; set; }
    }
    public sealed class StrategyJobDto
    {
        public string? Title { get; set; }
        public StrategyDto Strategy { get; set; } = new();
        public List<ItemDto> Items { get; set; } = new();
    }

    // ===== Runner =====
    public static class StrategyExample
    {
        public static object Run(List<StrategyJobDto> jobs)
        {
            var outs = new List<object>();

            foreach (var j in jobs ?? new())
            {
                try
                {
                    var items = (j.Items ?? new()).Select(x => new OrderItem(x.Sku, x.Qty, x.UnitPrice)).ToList();
                    var s = Make(j.Strategy);

                    var calc = new PriceCalculator(s);
                    var result = calc.Calc(items);

                    outs.Add(new { title = j.Title, result });
                }
                catch (Exception ex)
                {
                    outs.Add(new { title = j.Title, error = ex.Message });
                }
            }

            return new { count = outs.Count, results = outs };
        }

        private static IDiscountStrategy Make(StrategyDto d)
        {
            var t = (d?.Type ?? "none").Trim().ToLowerInvariant();
            return t switch
            {
                "none" => new NoDiscountStrategy(),
                "percent" => new PercentOffStrategy(d?.Percent ?? 0m),
                "fixed" => new FixedOffStrategy(d?.Amount ?? 0m),
                "bulk" => new BulkPercentStrategy(d?.MinQty ?? 5, d?.Percent ?? 0.1m),
                _ => throw new ArgumentException($"Unsupported strategy: {d?.Type}")
            };
        }
    }
}
