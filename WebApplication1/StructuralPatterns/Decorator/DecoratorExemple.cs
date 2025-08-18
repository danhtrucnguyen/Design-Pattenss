using WebApplication1.StructuralPatterns.Decorator;

namespace Design_Patterns.StructuralPatterns.Decorator
{
    // ===== DTOs cho Postman =====
    public sealed class OrderItemDto
    {
        public string Sku { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public sealed class PriceJobDto
    {
        public string Country { get; set; } = "US";
        // "Standard" | "Express"
        public string ShippingMethod { get; set; } = "Standard";
        public List<OrderItemDto> Items { get; set; } = new();

        // Các tham số cho decorator
        public decimal TaxRate { get; set; } = 0m;          // 0.08 = 8%
        public decimal CouponPercent { get; set; } = 0m;    // 0.10 = 10%
        // Thứ tự áp dụng decorators, mặc định: shipping -> tax -> coupon
        public List<string>? Pipeline { get; set; }
    }

    public static class DecoratorExample
    {
        public static object Run(List<PriceJobDto> jobs)
        {
            var results = new List<object>();

            foreach (var j in jobs)
            {
                try
                {
                    // Map DTO -> domain Order
                    var order = new Order
                    {
                        Country = j.Country,
                        ShippingMethod = ParseShipping(j.ShippingMethod)
                    };
                    foreach (var it in j.Items)
                        order.Items.Add(new OrderItem(it.Sku, it.Quantity, it.UnitPrice));

                    // Xây pipeline Decorator theo thứ tự yêu cầu
                    var steps = (j.Pipeline is { Count: > 0 })
                        ? j.Pipeline.Select(s => s.Trim().ToLowerInvariant()).ToList()
                        : new List<string> { "shipping", "tax", "coupon" };

                    IPriceCalculator calc = new BasePriceCalculator();
                    foreach (var s in steps)
                    {
                        switch (s)
                        {
                            case "shipping":
                                calc = new ShippingDecorator(calc);
                                break;
                            case "tax":
                                calc = new TaxDecorator(calc, _ => Clamp01(j.TaxRate));
                                break;
                            case "coupon":
                                calc = new CouponPercentDecorator(Clamp01(j.CouponPercent), calc);
                                break;
                            default:
                                throw new ArgumentException($"Unsupported step '{s}'. Use shipping|tax|coupon");
                        }
                    }

                    // Tính tổng qua decorator chain
                    var total = calc.Calculate(order);

                    // Tự tính breakdown (đúng theo logic các decorator ở trên)
                    var baseCalc = new BasePriceCalculator();
                    decimal subtotal = baseCalc.Calculate(order);

                    decimal shippingFee = steps.Contains("shipping")
                        ? (order.ShippingMethod == ShippingMethod.Express ? 15m : 5m)
                        : 0m;

                    decimal afterShipping = subtotal + shippingFee;

                    decimal tax = steps.Contains("tax") ? afterShipping * Clamp01(j.TaxRate) : 0m;
                    decimal afterTax = afterShipping + tax;

                    decimal discount = steps.Contains("coupon") ? afterTax * Clamp01(j.CouponPercent) : 0m;
                    decimal breakdownTotal = Math.Max(0, afterTax - discount);

                    // (Tuỳ chọn) kiểm chứng đồng nhất
                    // if (Math.Abs(total - breakdownTotal) > 0.0001m) { /* log cảnh báo */ }

                    results.Add(new
                    {
                        input = new
                        {
                            j.Country,
                            ShippingMethod = order.ShippingMethod.ToString(),
                            items = j.Items,
                            pipeline = steps,
                            taxRate = Clamp01(j.TaxRate),
                            couponPercent = Clamp01(j.CouponPercent)
                        },
                        breakdown = new
                        {
                            subtotal,
                            shippingFee,
                            tax,
                            discount,
                            total = breakdownTotal
                        },
                        totalByDecorator = total // cùng giá trị với breakdown.total
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new { error = ex.Message });
                }
            }

            return new { count = jobs.Count, results };
        }

        private static ShippingMethod ParseShipping(string s)
            => string.Equals(s, "express", StringComparison.OrdinalIgnoreCase)
               ? ShippingMethod.Express : ShippingMethod.Standard;

        private static decimal Clamp01(decimal v) => v < 0 ? 0 : (v > 1 ? 1 : v);
    }
}
