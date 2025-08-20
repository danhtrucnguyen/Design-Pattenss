using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.ChainEcom
{
    public sealed class EcomJobDto
    {
        public string? Title { get; set; }
        public List<OrderDto> Orders { get; set; } = new();
    }

    public static class ChainEcomExample
    {
        public static object Run(List<EcomJobDto> jobs)
        {
            var outputs = new List<object>();

            foreach (var j in jobs ?? new())
            {
                var chain = OrderValidationChains.Standard();
                var items = new List<object>();

                foreach (var ord in j.Orders ?? new())
                {
                    try
                    {
                        var res = chain.Handle(ord);
                        items.Add(new
                        {
                            email = ord.Email,
                            country = ord.Country,
                            payment = ord.Payment,
                            total = ord.Total,
                            items = ord.Items.Select(i => new { i.Sku, i.Qty, i.Stock }),
                            ok = res.Ok,
                            by = res.By,
                            message = res.Message,
                            trail = res.Trail
                        });
                    }
                    catch (Exception ex)
                    {
                        items.Add(new { error = ex.Message });
                    }
                }

                outputs.Add(new { title = j.Title, results = items });
            }

            return new { count = outputs.Count, results = outputs };
        }
    }
}
