using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.ChainEcom
{
    // ===== Domain =====
    public sealed class ItemDto { public string Sku { get; set; } = ""; public int Qty { get; set; } public int Stock { get; set; } }
    public sealed class OrderDto
    {
        public string Email { get; set; } = "";
        public string Country { get; set; } = "US";
        // "cod" | "card" | "paypal"
        public string Payment { get; set; } = "card";
        public decimal Total { get; set; }
        public List<ItemDto> Items { get; set; } = new();
    }

    public sealed record ValidationResult(bool Ok, string By, string Message, List<string> Trail);

    // ===== Handler contract =====
    public interface IOrderHandler
    {
        IOrderHandler SetNext(IOrderHandler next);
        ValidationResult Handle(OrderDto order);
    }

    // ===== Base handler =====
    public abstract class ValidatorBase : IOrderHandler
    {
        private IOrderHandler? _next;
        protected abstract string Name { get; }

        public IOrderHandler SetNext(IOrderHandler next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            return next;
        }

        public ValidationResult Handle(OrderDto o)
        {
            var trail = new List<string> { Name };
            var res = Check(o);
            if (!res.Ok) { res.Trail.Insert(0, Name); return res; }

            if (_next is null) return new ValidationResult(true, Name, "OK", trail);

            var nextRes = _next.Handle(o);
            nextRes.Trail.Insert(0, Name);
            return nextRes;
        }

        protected abstract ValidationResult Check(OrderDto o);
        protected static ValidationResult Fail(string by, string msg)
            => new(false, by, msg, new List<string>());
        protected static ValidationResult Pass(string by)
            => new(true, by, "OK", new List<string>());
    }

    // ===== Concrete handlers =====
    // 1) Cart not empty
    public sealed class CartNotEmptyHandler : ValidatorBase
    {
        protected override string Name => "CartNotEmpty";
        protected override ValidationResult Check(OrderDto o)
            => (o.Items != null && o.Items.Count > 0) ? Pass(Name) : Fail(Name, "Cart is empty");
    }

    // 2) Stock check
    public sealed class StockHandler : ValidatorBase
    {
        protected override string Name => "Stock";
        protected override ValidationResult Check(OrderDto o)
        {
            foreach (var it in o.Items ?? Enumerable.Empty<ItemDto>())
            {
                if (it.Qty <= 0) return Fail(Name, $"Invalid qty for {it.Sku}");
                if (it.Stock < it.Qty) return Fail(Name, $"Out of stock: {it.Sku}");
            }
            return Pass(Name);
        }
    }

    // 3) Country supported
    public sealed class CountryHandler : ValidatorBase
    {
        protected override string Name => "Country";
        private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase) { "US", "CA", "VN", "JP", "DE" };
        protected override ValidationResult Check(OrderDto o)
            => Supported.Contains((o.Country ?? "").Trim()) ? Pass(Name) : Fail(Name, $"Unsupported country: {o.Country}");
    }

    // 4) Payment limits
    public sealed class PaymentLimitHandler : ValidatorBase
    {
        protected override string Name => "PaymentLimit";
        protected override ValidationResult Check(OrderDto o)
        {
            var p = (o.Payment ?? "card").Trim().ToLowerInvariant();
            var limit = p switch { "cod" => 200m, "paypal" => 20000m, "card" => 50000m, _ => 0m };
            if (o.Total <= 0) return Fail(Name, "Total must be > 0");
            return o.Total <= limit ? Pass(Name) : Fail(Name, $"Payment '{p}' exceeds limit {limit}");
        }
    }

    // 5) Simple fraud rule
    public sealed class FraudHandler : ValidatorBase
    {
        protected override string Name => "Fraud";
        private static readonly HashSet<string> Blacklist = new(StringComparer.OrdinalIgnoreCase) { "fraud@badmail.com" };
        protected override ValidationResult Check(OrderDto o)
        {
            if (Blacklist.Contains((o.Email ?? "").Trim()))
                return Fail(Name, "Blacklisted email");
            if (o.Total >= 10000m && string.Equals(o.Payment, "cod", StringComparison.OrdinalIgnoreCase))
                return Fail(Name, "High amount cannot use COD");
            return Pass(Name);
        }
    }

    // Helper: build standard chain
    public static class OrderValidationChains
    {
        public static IOrderHandler Standard()
        {
            var h1 = new CartNotEmptyHandler();
            var h2 = new StockHandler();
            var h3 = new CountryHandler();
            var h4 = new PaymentLimitHandler();
            var h5 = new FraudHandler();

            h1.SetNext(h2).SetNext(h3).SetNext(h4).SetNext(h5);
            return h1;
        }
    }
}

//GET http://localhost:5102/api/chainecom/demo
//POST http://localhost:5102/api/chainecom/run

/*
[
  {
    "title": "Batch kiểm tra đơn",
    "orders": [
      {
        "email": "ok@buyer.com",
        "country": "VN",
        "payment": "cod",
        "total": 150,
        "items": [
          { "sku": "TS-01", "qty": 2, "stock": 5 }
        ]
      },
      {
        "email": "fraud@badmail.com",
        "country": "US",
        "payment": "card",
        "total": 500,
        "items": [
          { "sku": "NB-001", "qty": 1, "stock": 3 }
        ]
      },
      {
        "email": "big@order.com",
        "country": "US",
        "payment": "cod",
        "total": 12000,
        "items": [
          { "sku": "TV-70", "qty": 1, "stock": 2 }
        ]
      }
    ]
  }
]
*/