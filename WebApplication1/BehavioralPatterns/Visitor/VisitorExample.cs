using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.Visitor
{
    // ===== DTOs =====
    public sealed class ItemDto
    {
        // "physical" | "digital" | "service"
        public string Type { get; set; } = "physical";
        public string Sku { get; set; } = "";
        public int? Qty { get; set; }          // physical/digital
        public decimal? UnitPrice { get; set; }// physical/digital
        public decimal? WeightKg { get; set; } // physical
        public int? Hours { get; set; }        // service
        public decimal? HourlyRate { get; set; } // service
    }

    public sealed class VisitorJobDto
    {
        public string? Title { get; set; }
        public string? Coupon { get; set; } // SALE10 | SVC5 | null
        public List<ItemDto> Items { get; set; } = new();
    }

    public static class VisitorExample
    {
        public static object Run(List<VisitorJobDto> jobs)
        {
            var outs = new List<object>();
            foreach (var j in jobs ?? new())
            {
                try
                {
                    var cart = new CartAggregate();

                    foreach (var d in j.Items ?? new())
                    {
                        switch ((d.Type ?? "physical").Trim().ToLowerInvariant())
                        {
                            case "physical":
                                cart.Add(new PhysicalItem(
                                    d.Sku, d.Qty ?? 0, d.UnitPrice ?? 0m, d.WeightKg ?? 0m));
                                break;
                            case "digital":
                                cart.Add(new DigitalItem(
                                    d.Sku, d.Qty ?? 0, d.UnitPrice ?? 0m));
                                break;
                            case "service":
                                cart.Add(new ServiceItem(
                                    d.Sku, d.Hours ?? 0, d.HourlyRate ?? 0m));
                                break;
                            default:
                                throw new ArgumentException($"Unsupported type: {d.Type}");
                        }
                    }

                    var summary = new SummaryVisitor();
                    var discountV = new DiscountVisitor(j.Coupon);

                    cart.Accept(summary);
                    cart.Accept(discountV);

                    var totalAfterDiscount = Math.Max(0, summary.Subtotal - discountV.Discount) + summary.Shipping + summary.Tax;

                    outs.Add(new
                    {
                        title = j.Title,
                        coupon = j.Coupon,
                        items = cart.View(),
                        totals = new
                        {
                            subtotal = summary.Subtotal,
                            shipping = summary.Shipping,
                            tax = summary.Tax,
                            discount = discountV.Discount,
                            total = totalAfterDiscount
                        }
                    });
                }
                catch (Exception ex)
                {
                    outs.Add(new { title = j.Title, error = ex.Message });
                }
            }

            return new { count = outs.Count, results = outs };
        }
    }
}
