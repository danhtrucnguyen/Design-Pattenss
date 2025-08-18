using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.CretionalPatterns.Builder
{
    //DTO
    public sealed class OrderItemDto
    {
        public string Sku { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public sealed class BuildOrderJobDto
    {
        public string CustomerId { get; set; } = "";
        public List<OrderItemDto> Items { get; set; } = new();

        public string? CouponCode { get; set; }
        public decimal? CouponValue { get; set; }

        // "Standard" | "Express"
        public string ShippingMethod { get; set; } = "Standard";
        public string ShippingAddress { get; set; } = "";
    }

    public static class BuilderExample
    {
        public static object Run(List<BuildOrderJobDto> jobs)
        {
            var results = new List<object>();

            foreach (var j in jobs)
            {
                try
                {
                    var builder = new OrderBuilder()
                        .WithCustomer(j.CustomerId);

                    foreach (var it in j.Items)
                        builder.AddItem(it.Sku, it.Quantity, it.UnitPrice);

                    if (!string.IsNullOrWhiteSpace(j.CouponCode) && j.CouponValue.HasValue)
                        builder.WithCoupon(j.CouponCode!, Math.Max(0, j.CouponValue.Value));

                    var method = ParseShipping(j.ShippingMethod);
                    builder.WithShipping(method, j.ShippingAddress);

                    var order = builder.Build();

                    results.Add(new
                    {
                        input = j,
                        order = new
                        {
                            order.Id,
                            order.CustomerId,
                            items = order.Items.Select(x => new { x.Sku, x.Quantity, x.UnitPrice, x.Subtotal }),
                            order.Subtotal,
                            order.Discount,
                            order.ShippingFee,
                            order.Total,
                            ShippingMethod = order.ShippingMethod.ToString(),
                            order.ShippingAddress
                        }
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new { input = j, error = ex.Message });
                }
            }

            return new { count = results.Count, results };
        }

        private static ShippingMethod ParseShipping(string s)
            => string.Equals(s?.Trim(), "Express", StringComparison.OrdinalIgnoreCase)
               ? ShippingMethod.Express
               : ShippingMethod.Standard;
    }
}
