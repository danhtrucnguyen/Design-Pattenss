using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Mediator
{
    // IMediator: nhận Notify từ các colleague.
    public interface IMediator
    {
        void Notify(object sender, string evt); // "cart_changed" | "country_changed" | "coupon_changed"
        Summary GetSummary();
        void AddNote(string note);
    }

    public sealed record Summary(
        decimal Subtotal, decimal Discount, decimal Shipping, decimal Total,
        string Country, string? Coupon, IReadOnlyList<string> Notes,
        IReadOnlyList<object> ItemsView
    );

    //Colleague: Cart
    public sealed class Cart
    {
        private readonly IMediator _m;
        private readonly Dictionary<string, (int qty, decimal price, decimal weight)> _lines
            = new(StringComparer.OrdinalIgnoreCase);

        public Cart(IMediator m) => _m = m;

        public void Add(string sku, int qty, decimal unitPrice, decimal weightKg)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("sku required");
            if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
            if (unitPrice < 0 || weightKg < 0) throw new ArgumentOutOfRangeException();

            if (_lines.TryGetValue(sku, out var cur)) _lines[sku] = (cur.qty + qty, unitPrice, weightKg);
            else _lines[sku] = (qty, unitPrice, weightKg);

            _m.Notify(this, "cart_changed");
        }

        public void Remove(string sku, int qty)
        {
            if (!_lines.TryGetValue(sku, out var cur)) return;
            var take = Math.Min(qty, cur.qty);
            var remain = cur.qty - take;
            if (remain <= 0) _lines.Remove(sku);
            else _lines[sku] = (remain, cur.price, cur.weight);

            _m.Notify(this, "cart_changed");
        }

        public decimal Subtotal => _lines.Sum(kv => kv.Value.qty * kv.Value.price);
        public decimal Weight => _lines.Sum(kv => kv.Value.qty * kv.Value.weight);
        public IReadOnlyDictionary<string, (int qty, decimal price, decimal weight)> Lines => _lines;
    }

    // Colleague: Shipping
    public sealed class Shipping
    {
        private readonly IMediator _m;
        public Shipping(IMediator m) => _m = m;

        private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase) { "US", "VN", "JP" };
        public string Country { get; private set; } = "US";

        public void SetCountry(string country)
        {
            Country = (country ?? "US").Trim().ToUpperInvariant();
            _m.Notify(this, "country_changed");
        }

        public (bool ok, decimal fee, string? error) Compute(decimal subtotal, decimal weight, bool freeShip)
        {
            if (!Supported.Contains(Country)) return (false, 0m, $"Unsupported country: {Country}");
            if (freeShip) return (true, 0m, null);

            //US=5, VN=3, JP=7; free nếu subtotal >= 200
            if (subtotal >= 200m) return (true, 0m, null);
            var fee = Country == "US" ? 5m : Country == "VN" ? 3m : 7m;
            return (true, fee, null);
        }
    }

    //Colleague: Coupon
    public sealed class Coupon
    {
        private readonly IMediator _m;
        public Coupon(IMediator m) => _m = m;

        public string? Code { get; private set; }
        public decimal Percent { get; private set; } // SALE10 = 0.10
        public bool FreeShip { get; private set; }

        public void Apply(string code)
        {
            Code = (code ?? "").Trim().ToUpperInvariant();
            Percent = 0m; FreeShip = false;

            switch (Code)
            {
                case "SALE10": Percent = 0.10m; break;
                case "SALE15": Percent = 0.15m; break;
                case "FREESHIP": FreeShip = true; break;
                case "": Code = null; break; // clear
                default:
                    _m.AddNote($"Unknown coupon: {Code}");
                    Code = null;
                    break;
            }
            _m.Notify(this, "coupon_changed");
        }
    }

    //Concrete Mediator
    public sealed class CheckoutMediator : IMediator
    {
        private readonly List<string> _notes = new();
        public Cart Cart { get; }
        public Shipping Shipping { get; }
        public Coupon Coupon { get; }

        // cache tạm thời
        private decimal _subtotal, _discount, _shipping, _total;
        private bool _validCountry = true;

        public CheckoutMediator()
        {
            Cart = new Cart(this);
            Shipping = new Shipping(this);
            Coupon = new Coupon(this);
            Recalc();
        }

        public void Notify(object sender, string evt)
        {
           
            Recalc();
        }

        public void AddNote(string note)
        {
            if (!string.IsNullOrWhiteSpace(note)) _notes.Add(note);
        }

        private void Recalc()
        {
            _notes.Clear();
            _subtotal = Cart.Subtotal;

            // Discount: phần trăm, giới hạn tối đa 50
            _discount = 0m;
            if (!string.IsNullOrEmpty(Coupon.Code) && Coupon.Percent > 0)
                _discount = Math.Min(Math.Round(_subtotal * Coupon.Percent, 2, MidpointRounding.AwayFromZero), 50m);

            // Shipping
            var ship = Shipping.Compute(subtotal: Math.Max(0, _subtotal - _discount), weight: Cart.Weight, freeShip: Coupon.FreeShip);
            _validCountry = ship.ok;
            _shipping = ship.ok ? ship.fee : 0m;
            if (!ship.ok && ship.error is not null) _notes.Add(ship.error);

            // Total
            _total = Math.Max(0, _subtotal - _discount) + _shipping;
        }

        public Summary GetSummary()
        {
            var itemsView = Cart.Lines.Select(kv => new {
                sku = kv.Key,
                quantity = kv.Value.qty,
                unitPrice = kv.Value.price,
                weightKg = kv.Value.weight,
                lineTotal = kv.Value.qty * kv.Value.price
            }).Cast<object>().ToList();

            return new Summary(
                Subtotal: _subtotal,
                Discount: _discount,
                Shipping: _shipping,
                Total: _total,
                Country: Shipping.Country,
                Coupon: Coupon.Code,
                Notes: _notes.ToList(),
                ItemsView: itemsView
            );
        }
    }
}

//GET http://localhost:5102/api/mediator/demo
//POST http://localhost:5102/api/mediator/run

/*
[
  {
    "title": "Flow checkout ngắn",
    "actions": [
      { "type": "add", "sku": "A001", "qty": 2, "unitPrice": 50,  "weightKg": 0.5 },
      { "type": "add", "sku": "B002", "qty": 1, "unitPrice": 200, "weightKg": 1.2 },
      { "type": "country", "country": "US" },
      { "type": "coupon", "code": "SALE10" },
      { "type": "coupon", "code": "FREESHIP" },
      { "type": "remove", "sku": "A001", "qty": 1 }
    ]
  }
]
*/