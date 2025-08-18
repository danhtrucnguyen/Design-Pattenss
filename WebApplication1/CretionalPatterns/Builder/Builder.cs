using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.CretionalPatterns.Builder
{
    public enum ShippingMethod { Standard, Express }

    public sealed record OrderItem(string Sku, int Quantity, decimal UnitPrice)
    {
        public decimal Subtotal => Quantity * UnitPrice;
    }

    public sealed record Order(
        string Id,
        string CustomerId,
        IReadOnlyList<OrderItem> Items,
        decimal Subtotal,
        decimal Discount,
        decimal ShippingFee,
        decimal Total,
        ShippingMethod ShippingMethod,
        string ShippingAddress
    );

    public interface IOrderBuilder
    {
        IOrderBuilder WithCustomer(string id);
        IOrderBuilder AddItem(string sku, int qty, decimal unitPrice);
        IOrderBuilder WithCoupon(string code, decimal value);       // giảm tuyệt đối
        IOrderBuilder WithShipping(ShippingMethod method, string address);
        Order Build();
    }

    public sealed class OrderBuilder : IOrderBuilder
    {
        private string _customerId = "";
        private readonly List<OrderItem> _items = new();
        private string? _couponCode;
        private decimal _discount;
        private ShippingMethod _shippingMethod = ShippingMethod.Standard;
        private string _shippingAddress = "";

        public IOrderBuilder WithCustomer(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Customer id is required");
            _customerId = id.Trim();
            return this;
        }

        public IOrderBuilder AddItem(string sku, int qty, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU is required");
            if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
            if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));

            _items.Add(new OrderItem(sku.Trim(), qty, unitPrice));
            return this;
        }

        public IOrderBuilder WithCoupon(string code, decimal value)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Coupon code is required");
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

            _couponCode = code.Trim();
            _discount = value;
            return this;
        }

        public IOrderBuilder WithShipping(ShippingMethod method, string address)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Shipping address is required");
            _shippingMethod = method;
            _shippingAddress = address.Trim();
            return this;
        }

        public Order Build()
        {
            if (string.IsNullOrWhiteSpace(_customerId))
                throw new InvalidOperationException("Customer is required");
            if (_items.Count == 0)
                throw new InvalidOperationException("At least one item is required");

            var subtotal = _items.Sum(i => i.Subtotal);
            var discount = Math.Min(_discount, subtotal);
            var shippingFee = ShippingFeeFor(_shippingMethod);
            var total = Math.Max(0, subtotal - discount) + shippingFee;

            return new Order(
                Id: Guid.NewGuid().ToString("N"),
                CustomerId: _customerId,
                Items: _items.ToList().AsReadOnly(),
                Subtotal: subtotal,
                Discount: discount,
                ShippingFee: shippingFee,
                Total: total,
                ShippingMethod: _shippingMethod,
                ShippingAddress: _shippingAddress
            );
        }

        private static decimal ShippingFeeFor(ShippingMethod method)
            => method == ShippingMethod.Express ? 15m : 5m;
    }
}


//GET http://localhost:5102/api/builder/demo
//POST http://localhost:5102/api/builder/build

/*
[
  {
    "customerId": "CUST001",
    "items": [
      { "sku": "SKU123", "quantity": 2, "unitPrice": 50 },
      { "sku": "SKU999", "quantity": 1, "unitPrice": 120 }
    ],
    "couponCode": "SALE10",
    "couponValue": 10,
    "shippingMethod": "Express",
    "shippingAddress": "123 Nga Tu So Street"
  },
  {
    "customerId": "CUST002",
    "items": [
      { "sku": "A1", "quantity": 3, "unitPrice": 20 }
    ],
    "shippingMethod": "Standard",
    "shippingAddress": "456 Le Loi, Da Nang"
  }
]
*/