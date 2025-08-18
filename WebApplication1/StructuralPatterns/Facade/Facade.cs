using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.StructuralPatterns.Facade
{
    // ----- Contracts -----
    public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice)
    {
        public decimal Subtotal => checked(Quantity * UnitPrice);
    }

    public sealed record Address(string Recipient, string Line1, string City, string Country, string Email);

    public enum PaymentMethod { Visa, Paypal, Cod }

    public sealed record CheckoutRequest(
        IReadOnlyList<OrderLine> Lines,
        Address ShipTo,
        PaymentMethod Method
    );

    public sealed record CheckoutResult(
        bool Success, string? OrderId, string? TrackingId, string Message
    );

    public sealed record PaymentResult(bool Success, string Provider, string Message);

    public interface IInventoryService
    {
        bool Reserve(IEnumerable<OrderLine> lines);
    }

    public interface IPaymentService
    {
        PaymentResult Charge(decimal amount, PaymentMethod method);
    }

    public interface IShippingService
    {
        string CreateShipment(Address to);
    }

    public interface INotificationService
    {
        void SendEmail(string to, string subject, string body);
    }

    // ----- Facade -----
    public sealed class CheckoutFacade
    {
        private readonly IInventoryService _inventory;
        private readonly IPaymentService _payment;
        private readonly IShippingService _shipping;
        private readonly INotificationService _notify;

        public CheckoutFacade(
            IInventoryService inventory,
            IPaymentService payment,
            IShippingService shipping,
            INotificationService notify)
        {
            _inventory = inventory;
            _payment = payment;
            _shipping = shipping;
            _notify = notify;
        }

        public CheckoutResult PlaceOrder(CheckoutRequest req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (req.Lines is null || req.Lines.Count == 0)
                return new(false, null, null, "Cart is empty");

            // 1) Reserve inventory
            if (!_inventory.Reserve(req.Lines))
                return new(false, null, null, "Out of stock");

            // 2) Charge payment
            var amount = req.Lines.Sum(l =>
            {
                if (l.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(l.Quantity));
                if (l.UnitPrice < 0) throw new ArgumentOutOfRangeException(nameof(l.UnitPrice));
                return l.Subtotal;
            });

            var payRes = req.Method == PaymentMethod.Cod
                ? new PaymentResult(true, "COD", "Payment on delivery")
                : _payment.Charge(amount, req.Method);

            if (!payRes.Success)
                return new(false, null, null, $"Payment failed: {payRes.Message}");

            // 3) Shipment
            var tracking = _shipping.CreateShipment(req.ShipTo);

            // 4) Notify (best effort)
            try
            {
                _notify.SendEmail(req.ShipTo.Email, "Order confirmed",
                    $"Your order has been placed. Tracking: {tracking}");
            }
            catch { }

            // 5) Result
            var orderId = Guid.NewGuid().ToString("N").ToUpperInvariant();
            return new(true, orderId, tracking, $"Order placed via {payRes.Provider}");
        }
    }

    // ---- Fake implementations for demo ----
    public sealed class MemoryInventory : IInventoryService
    {
        private readonly Dictionary<string, int> _stock;
        public MemoryInventory(IDictionary<string, int> initial)
            => _stock = new(initial ?? new Dictionary<string, int>());

        public bool Reserve(IEnumerable<OrderLine> lines)
        {
            foreach (var line in lines)
                if (!_stock.TryGetValue(line.Sku, out var qty) || qty < line.Quantity)
                    return false;

            foreach (var line in lines)
                _stock[line.Sku] -= line.Quantity;

            return true;
        }
    }

    public sealed class SimplePayment : IPaymentService
    {
        private readonly bool _shouldFail;
        public SimplePayment(bool shouldFail = false) => _shouldFail = shouldFail;

        public PaymentResult Charge(decimal amount, PaymentMethod method)
        {
            if (_shouldFail) return new(false, method.ToString(), "Gateway error");
            if (amount <= 0) return new(false, method.ToString(), "Invalid amount");
            return new(true, method.ToString(), $"Charged {amount} via {method}");
        }
    }

    public sealed class DummyShipping : IShippingService
    {
        public string CreateShipment(Address to)
            => $"TRK-{to.City.ToUpperInvariant()}-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    public sealed class MemoryNotification : INotificationService
    {
        public readonly List<(string To, string Subj, string Body)> Sent = new();
        public void SendEmail(string to, string subject, string body) => Sent.Add((to, subject, body));
    }
}

//GET http://localhost:5102/api/facade/demo
//POST http://localhost:5102/api/facade/checkout

/*
[
  {
    "lines": [
      { "sku": "SKU-001", "quantity": 2, "unitPrice": 50 },
      { "sku": "SKU-002", "quantity": 1, "unitPrice": 100 }
    ],
    "shipTo": {
      "recipient": "John Doe",
      "line1": "123 Main Street",
      "city": "New York",
      "country": "US",
      "email": "john@example.com"
    },
    "method": "visa",
    "initialStock": { "SKU-001": 10, "SKU-002": 5 },
    "paymentShouldFail": false
  },
  {
    "lines": [
      { "sku": "A1", "quantity": 1, "unitPrice": 20 }
    ],
    "shipTo": {
      "recipient": "Jane",
      "line1": "1 First Ave",
      "city": "Hanoi",
      "country": "VN",
      "email": "jane@example.com"
    },
    "method": "paypal",
    "initialStock": { "A1": 5 },
    "paymentShouldFail": true
  }
]
*/