using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.StructuralPatterns.Facade
{
    // ---- GIỮ 1 BẢN DUY NHẤT các DTO dưới đây ----
    public sealed class OrderLineDto
    {
        public string Sku { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public sealed class AddressDto
    {
        public string Recipient { get; set; } = "";
        public string Line1 { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public sealed class CheckoutJobDto
    {
        public List<OrderLineDto> Lines { get; set; } = new();
        public AddressDto ShipTo { get; set; } = new();
        public string Method { get; set; } = "visa";          // "visa" | "paypal" | "cod"
        public Dictionary<string, int>? InitialStock { get; set; }
        public bool PaymentShouldFail { get; set; } = false;
    }

    public static class FacadeExample
    {
        // Nếu vẫn còn mơ hồ, bạn có thể ghim FQN ngay tại đây:
        // public static object Run(List<WebApplication1.StructuralPatterns.Facade.CheckoutJobDto> jobs)
        public static object Run(List<CheckoutJobDto> jobs)
        {
            var results = new List<object>();
            foreach (var j in jobs)
            {
                try
                {
                    var stock = j.InitialStock ?? j.Lines
                        .GroupBy(x => x.Sku)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity) * 2);

                    var inventory = new MemoryInventory(stock);
                    var payment = new SimplePayment(j.PaymentShouldFail);
                    var shipping = new DummyShipping();
                    var notify = new MemoryNotification();

                    var facade = new CheckoutFacade(inventory, payment, shipping, notify);

                    var lines = j.Lines.Select(l => new OrderLine(l.Sku, l.Quantity, l.UnitPrice)).ToList();
                    var addr = new Address(j.ShipTo.Recipient, j.ShipTo.Line1, j.ShipTo.City, j.ShipTo.Country, j.ShipTo.Email);
                    var method = ParseMethod(j.Method);

                    var req = new CheckoutRequest(lines, addr, method);
                    var res = facade.PlaceOrder(req);

                    var amount = lines.Sum(x => x.Subtotal);

                    results.Add(new
                    {
                        input = new { method = method.ToString(), amount, shipTo = j.ShipTo, lines = j.Lines },
                        result = new { res.Success, res.OrderId, res.TrackingId, res.Message },
                        emails = notify.Sent.Select(e => new { to = e.To, subject = e.Subj, body = e.Body })
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new { error = ex.Message });
                }
            }
            return new { count = jobs.Count, results };
        }

        private static PaymentMethod ParseMethod(string s) =>
            s?.Trim().ToLowerInvariant() switch
            {
                "visa" => PaymentMethod.Visa,
                "paypal" => PaymentMethod.Paypal,
                "cod" => PaymentMethod.Cod,
                _ => throw new ArgumentException($"Unsupported payment method: {s}")
            };
    }
}
