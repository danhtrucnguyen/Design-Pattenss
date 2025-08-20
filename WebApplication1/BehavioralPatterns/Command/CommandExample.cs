using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Command
{
    // ===== DTOs để gọi từ Postman =====
    public sealed class CommandDto
    {
        // "add" | "remove" | "coupon" | "undo"
        public string Type { get; set; } = "add";
        public string? Sku { get; set; }
        public int? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Percent { get; set; }   // 0..1 cho coupon
        public string? Code { get; set; }
        public int? Steps { get; set; }         // cho undo
    }

    public sealed class CommandJobDto
    {
        public string? Title { get; set; }
        public List<CommandDto> Commands { get; set; } = new();
    }

    public static class CommandExample
    {
        public static object Run(List<CommandJobDto> jobs)
        {
            var outputs = new List<object>();

            foreach (var job in jobs ?? new())
            {
                var cart = new Cart();
                var bus = new CommandInvoker();
                var logs = new List<object>();

                foreach (var d in job.Commands ?? new())
                {
                    try
                    {
                        switch ((d.Type ?? "").Trim().ToLowerInvariant())
                        {
                            case "add":
                                bus.Do(new AddItemCommand(cart,
                                    sku: Req(d.Sku, "sku"),
                                    qty: d.Qty ?? 0,
                                    price: d.UnitPrice ?? 0m));
                                logs.Add(new { done = "add", sku = d.Sku, qty = d.Qty, unitPrice = d.UnitPrice });
                                break;

                            case "remove":
                                bus.Do(new RemoveItemCommand(cart,
                                    sku: Req(d.Sku, "sku"),
                                    qty: d.Qty ?? 0));
                                logs.Add(new { done = "remove", sku = d.Sku, qty = d.Qty });
                                break;

                            case "coupon":
                                bus.Do(new ApplyCouponCommand(cart,
                                    percent: d.Percent ?? 0m,
                                    code: d.Code ?? "COUPON"));
                                logs.Add(new { done = "coupon", code = d.Code, percent = d.Percent });
                                break;

                            case "undo":
                                var steps = Math.Max(1, d.Steps ?? 1);
                                var undone = bus.Undo(steps);
                                logs.Add(new { done = "undo", steps, undone });
                                break;

                            default:
                                logs.Add(new { error = $"unsupported command: {d.Type}" });
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logs.Add(new { command = d.Type, error = ex.Message });
                    }
                }

                outputs.Add(new
                {
                    title = job.Title,
                    history = bus.Executed,
                    cart = View(cart),
                    logs
                });
            }

            return new { count = outputs.Count, results = outputs };
        }

        private static object View(Cart c) => new
        {
            items = c.Lines.Select(kv => new {
                sku = kv.Key,
                quantity = kv.Value.qty,
                unitPrice = kv.Value.price,
                lineTotal = kv.Value.qty * kv.Value.price
            }),
            subtotal = c.Subtotal,
            discountPercent = c.DiscountPercent,
            discount = c.Discount,
            coupon = c.CouponCode,
            total = c.Total
        };

        private static string Req(string? s, string field)
            => string.IsNullOrWhiteSpace(s) ? throw new ArgumentException($"{field} required") : s.Trim();
    }
}
