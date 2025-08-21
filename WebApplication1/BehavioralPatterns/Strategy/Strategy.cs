using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Strategy
{
    // ===== Domain =====
    public sealed record OrderItem(string Sku, int Qty, decimal UnitPrice)
    {
        public decimal Line => checked(Qty * UnitPrice);
    }

    // ===== Strategy contract =====
    public interface IDiscountStrategy
    {
        string Name { get; }
        decimal GetDiscount(IReadOnlyList<OrderItem> items, decimal subtotal);
    }

    // ===== Concrete Strategies =====
    public sealed class NoDiscountStrategy : IDiscountStrategy
    {
        public string Name => "NONE";
        public decimal GetDiscount(IReadOnlyList<OrderItem> items, decimal subtotal) => 0m;
    }

    public sealed class PercentOffStrategy : IDiscountStrategy
    {
        private readonly decimal _percent; // 0..1
        public PercentOffStrategy(decimal percent) => _percent = Math.Clamp(percent, 0m, 1m);
        public string Name => $"PERCENT({_percent:P0})";
        public decimal GetDiscount(IReadOnlyList<OrderItem> items, decimal subtotal)
            => Math.Round(subtotal * _percent, 2, MidpointRounding.AwayFromZero);
    }

    public sealed class FixedOffStrategy : IDiscountStrategy
    {
        private readonly decimal _amount;
        public FixedOffStrategy(decimal amount) => _amount = Math.Max(0, amount);
        public string Name => $"FIXED({_amount})";
        public decimal GetDiscount(IReadOnlyList<OrderItem> items, decimal subtotal)
            => Math.Min(_amount, Math.Max(0, subtotal)); // không vượt quá subtotal
    }

    public sealed class BulkPercentStrategy : IDiscountStrategy
    {
        private readonly int _minQty;
        private readonly decimal _percent; // 0..1
        public BulkPercentStrategy(int minQty, decimal percent)
        {
            _minQty = Math.Max(1, minQty);
            _percent = Math.Clamp(percent, 0m, 1m);
        }
        public string Name => $"BULK(qty>={_minQty},{_percent:P0})";
        public decimal GetDiscount(IReadOnlyList<OrderItem> items, decimal subtotal)
        {
            var totalQty = items.Sum(x => x.Qty);
            if (totalQty < _minQty) return 0m;
            return Math.Round(subtotal * _percent, 2, MidpointRounding.AwayFromZero);
        }
    }

    // ===== Context =====
    public sealed class PriceCalculator
    {
        private readonly IDiscountStrategy _strategy;
        public PriceCalculator(IDiscountStrategy strategy) => _strategy = strategy ?? new NoDiscountStrategy();

        public object Calc(IReadOnlyList<OrderItem> items)
        {
            if (items is null || items.Count == 0)
                throw new InvalidOperationException("Items required");

            // Validate nhanh
            foreach (var it in items)
            {
                if (it.Qty <= 0) throw new ArgumentOutOfRangeException(nameof(it.Qty));
                if (it.UnitPrice < 0) throw new ArgumentOutOfRangeException(nameof(it.UnitPrice));
            }

            var subtotal = items.Sum(i => i.Line);
            var discount = Math.Min(subtotal, Math.Max(0, _strategy.GetDiscount(items, subtotal)));
            var total = Math.Max(0, subtotal - discount);

            return new
            {
                strategy = _strategy.Name,
                lines = items.Select(i => new { i.Sku, i.Qty, i.UnitPrice, lineTotal = i.Line }),
                subtotal,
                discount,
                total
            };
        }
    }
}

//GET http://localhost:5102/api/strategy/demo
//POST http://localhost:5102/api/strategy/run

/*
 [
  {
    "title": "Giảm 15% cho đơn",
    "strategy": { "type": "percent", "percent": 0.15 },
    "items": [
      { "sku": "A001", "qty": 2, "unitPrice": 50 },
      { "sku": "B002", "qty": 1, "unitPrice": 200 }
    ]
  },
  {
    "title": "Bulk ≥ 4 giảm 10%",
    "strategy": { "type": "bulk", "percent": 0.10, "minQty": 4 },
    "items": [
      { "sku": "C003", "qty": 3, "unitPrice": 40 },
      { "sku": "D004", "qty": 1, "unitPrice": 20 }
    ]
  }
]
*/