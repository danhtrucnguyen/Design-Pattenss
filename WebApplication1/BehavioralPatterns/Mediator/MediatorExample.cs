using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.Mediator
{
    // ===== DTOs cho Postman =====
    public sealed class ActionDto
    {
        // "add" | "remove" | "country" | "coupon"
        public string Type { get; set; } = "add";
        public string? Sku { get; set; }
        public int? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? WeightKg { get; set; }
        public string? Country { get; set; }
        public string? Code { get; set; }
    }

    public sealed class MediatorJobDto
    {
        public string? Title { get; set; }
        public List<ActionDto> Actions { get; set; } = new();
    }

    public static class MediatorExample
    {
        public static object Run(List<MediatorJobDto> jobs)
        {
            var outs = new List<object>();

            foreach (var j in jobs ?? new())
            {
                var mediator = new CheckoutMediator();
                var steps = new List<object>();

                // Chạy các action & chụp summary sau mỗi bước
                foreach (var a in j.Actions ?? new())
                {
                    try
                    {
                        switch ((a.Type ?? "").Trim().ToLowerInvariant())
                        {
                            case "add":
                                mediator.Cart.Add(
                                    sku: Req(a.Sku, "sku"),
                                    qty: a.Qty ?? 0,
                                    unitPrice: a.UnitPrice ?? 0m,
                                    weightKg: a.WeightKg ?? 0m
                                );
                                steps.Add(new { done = "add", a.Sku, a.Qty, a.UnitPrice, a.WeightKg, summary = mediator.GetSummary() });
                                break;

                            case "remove":
                                mediator.Cart.Remove(Req(a.Sku, "sku"), a.Qty ?? 0);
                                steps.Add(new { done = "remove", a.Sku, a.Qty, summary = mediator.GetSummary() });
                                break;

                            case "country":
                                mediator.Shipping.SetCountry(Req(a.Country, "country"));
                                steps.Add(new { done = "country", a.Country, summary = mediator.GetSummary() });
                                break;

                            case "coupon":
                                mediator.Coupon.Apply(a.Code ?? "");
                                steps.Add(new { done = "coupon", a.Code, summary = mediator.GetSummary() });
                                break;

                            default:
                                steps.Add(new { error = $"unsupported action: {a.Type}" });
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        steps.Add(new { action = a.Type, error = ex.Message });
                    }
                }

                outs.Add(new
                {
                    title = j.Title,
                    final = mediator.GetSummary(),
                    steps
                });
            }

            return new { count = outs.Count, results = outs };
        }

        private static string Req(string? s, string field)
            => string.IsNullOrWhiteSpace(s) ? throw new ArgumentException($"{field} required") : s.Trim();
    }
}
