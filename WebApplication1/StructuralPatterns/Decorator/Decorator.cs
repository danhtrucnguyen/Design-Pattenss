using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.StructuralPatterns.Decorator
{
    public enum ShippingMethod { Standard, Express }

    public sealed record OrderItem(string Sku, int Quantity, decimal UnitPrice)
    {
        public decimal Subtotal => Quantity * UnitPrice;
    }

    public sealed class Order
    {
        public List<OrderItem> Items { get; } = new();
        public string Country { get; init; } = "US";
        public ShippingMethod ShippingMethod { get; init; } = ShippingMethod.Standard;
    }

    // Target
    public interface IPriceCalculator
    {
        decimal Calculate(Order o);
    }

    // Concrete Component
    public sealed class BasePriceCalculator : IPriceCalculator
    {
        public decimal Calculate(Order o)
        {
            if (o is null) throw new ArgumentNullException(nameof(o));
            var subtotal = o.Items.Sum(i =>
            {
                if (i.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(i.Quantity));
                if (i.UnitPrice < 0) throw new ArgumentOutOfRangeException(nameof(i.UnitPrice));
                return i.Subtotal;
            });
            return subtotal;
        }
    }

    // Base Decorator
    public abstract class PriceDecorator : IPriceCalculator
    {
        protected readonly IPriceCalculator Inner;
        protected PriceDecorator(IPriceCalculator inner)
            => Inner = inner ?? throw new ArgumentNullException(nameof(inner));

        public virtual decimal Calculate(Order o) => Inner.Calculate(o);
    }

    public sealed class ShippingDecorator : PriceDecorator
    {
        public ShippingDecorator(IPriceCalculator inner) : base(inner) { }
        public override decimal Calculate(Order o)
        {
            var baseTotal = base.Calculate(o);
            var fee = o.ShippingMethod == ShippingMethod.Express ? 15m : 5m;
            return baseTotal + fee;
        }
    }

    public sealed class TaxDecorator : PriceDecorator
    {
        private readonly Func<Order, decimal> _rateProvider; // 0.08 = 8%
        public TaxDecorator(IPriceCalculator inner, Func<Order, decimal> rateProvider)
            : base(inner)
        {
            _rateProvider = rateProvider ?? (_ => 0m);
        }

        public override decimal Calculate(Order o)
        {
            var baseTotal = base.Calculate(o);
            var rate = Math.Clamp(_rateProvider(o), 0m, 1m);
            return baseTotal + baseTotal * rate;
        }
    }

    public sealed class CouponPercentDecorator : PriceDecorator
    {
        private readonly decimal _percent; // 0.10 = 10%
        public CouponPercentDecorator(decimal percent, IPriceCalculator inner) : base(inner)
        {
            _percent = Math.Clamp(percent, 0m, 1m);
        }
        public override decimal Calculate(Order o)
        {
            var baseTotal = base.Calculate(o);
            var discount = baseTotal * _percent;
            var result = baseTotal - discount;
            return result < 0 ? 0 : result;
        }
    }
}

//GET http://localhost:5102/api/decorator/demo
//POST http://localhost:5102/api/decorator/price

/*
[
  {
    "country": "US",
    "shippingMethod": "Express",
    "items": [
      { "sku": "SKU-001", "quantity": 2, "unitPrice": 50 },
      { "sku": "SKU-002", "quantity": 1, "unitPrice": 100 }
    ],
    "taxRate": 0.08,
    "couponPercent": 0.10,
    "pipeline": ["shipping", "tax", "coupon"]
  },
  {
    "country": "US",
    "shippingMethod": "Standard",
    "items": [
      { "sku": "A1", "quantity": 1, "unitPrice": 30 },
      { "sku": "B2", "quantity": 3, "unitPrice": 20 }
    ],
    "taxRate": 0.00,
    "couponPercent": 0.20,
    "pipeline": ["shipping", "coupon"]
  }
]
*/