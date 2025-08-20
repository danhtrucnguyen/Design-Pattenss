using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Command
{
    //DOMAIN (Receiver)
    public sealed class Cart
    {
        private readonly Dictionary<string, (int qty, decimal price)> _lines =
            new(StringComparer.OrdinalIgnoreCase);

        public void Add(string sku, int qty, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("sku required");
            if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
            if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));

            if (_lines.TryGetValue(sku, out var cur))
                _lines[sku] = (cur.qty + qty, unitPrice);
            else
                _lines[sku] = (qty, unitPrice);
        }

        public int Remove(string sku, int qty)
        {
            if (!_lines.TryGetValue(sku, out var cur)) return 0;
            var take = Math.Min(qty, cur.qty);
            var remain = cur.qty - take;
            if (remain <= 0) _lines.Remove(sku);
            else _lines[sku] = (remain, cur.price);
            return take; // số lượng thực sự đã trừ (để Undo chuẩn)
        }

        public IReadOnlyDictionary<string, (int qty, decimal price)> Lines => _lines;

        // Discount theo %
        public decimal DiscountPercent { get; private set; } = 0m;
        public string? CouponCode { get; private set; }

        public void ApplyPercent(decimal percent, string code)
        {
            DiscountPercent = Math.Clamp(percent, 0m, 1m);
            CouponCode = code?.Trim();
        }
        public void ClearCoupon()
        {
            DiscountPercent = 0m;
            CouponCode = null;
        }

        public decimal Subtotal => _lines.Sum(kv => kv.Value.qty * kv.Value.price);
        public decimal Discount => Math.Round(Subtotal * DiscountPercent, 2, MidpointRounding.AwayFromZero);
        public decimal Total => Subtotal - Discount;
    }

    //COMMAND (ICommand + Concrete)
    public interface ICartCommand
    {
        string Name { get; }
        void Execute();
        void Undo();
    }

    public sealed class AddItemCommand : ICartCommand
    {
        private readonly Cart _cart;
        private readonly string _sku;
        private readonly int _qty;
        private readonly decimal _price;
        public string Name => $"ADD({_sku} x{_qty} @{_price})";

        public AddItemCommand(Cart cart, string sku, int qty, decimal price)
        { _cart = cart; _sku = sku; _qty = qty; _price = price; }

        public void Execute() => _cart.Add(_sku, _qty, _price);
        public void Undo() => _cart.Remove(_sku, _qty);
    }

    public sealed class RemoveItemCommand : ICartCommand
    {
        private readonly Cart _cart;
        private readonly string _sku;
        private readonly int _qty;
        private int _actuallyRemoved; // để undo đúng số lượng
        private decimal _lastPrice;
        public string Name => $"REMOVE({_sku} x{_qty})";

        public RemoveItemCommand(Cart cart, string sku, int qty)
        { _cart = cart; _sku = sku; _qty = qty; }

        public void Execute()
        {
            _lastPrice = _cart.Lines.TryGetValue(_sku, out var v) ? v.price : 0m;
            _actuallyRemoved = _cart.Remove(_sku, _qty);
        }
        public void Undo()
        {
            if (_actuallyRemoved > 0)
                _cart.Add(_sku, _actuallyRemoved, _lastPrice);
        }
    }

    public sealed class ApplyCouponCommand : ICartCommand
    {
        private readonly Cart _cart;
        private readonly decimal _percent;
        private readonly string _code;
        private decimal _prevPercent;
        private string? _prevCode;

        public string Name => $"COUPON({_code}:{_percent:P0})";

        public ApplyCouponCommand(Cart cart, decimal percent, string code)
        { _cart = cart; _percent = percent; _code = code ?? "COUPON"; }

        public void Execute()
        {
            _prevPercent = _cart.DiscountPercent;
            _prevCode = _cart.CouponCode;
            _cart.ApplyPercent(_percent, _code);
        }
        public void Undo() => _cart.ApplyPercent(_prevPercent, _prevCode ?? "");
    }

    //INVOKER (lưu lịch sử để Undo)
    public sealed class CommandInvoker
    {
        private readonly Stack<ICartCommand> _history = new();
        public List<string> Executed { get; } = new();

        public void Do(ICartCommand cmd)
        {
            cmd.Execute();
            _history.Push(cmd);
            Executed.Add(cmd.Name);
        }

        public List<string> Undo(int steps = 1)
        {
            var undone = new List<string>();
            for (int i = 0; i < steps && _history.Count > 0; i++)
            {
                var cmd = _history.Pop();
                cmd.Undo();
                undone.Add(cmd.Name);
            }
            return undone;
        }
    }
}

//GET http://localhost:5102/api/command/demo
//POST http://localhost:5102/api/command/run

/*
[
  {
    "title": "Checkout flow",
    "commands": [
      { "type": "add", "sku": "A001", "qty": 2, "unitPrice": 50 },
      { "type": "add", "sku": "B002", "qty": 1, "unitPrice": 200 },
      { "type": "coupon", "percent": 0.10, "code": "SALE10" },
      { "type": "remove", "sku": "A001", "qty": 1 },
      { "type": "undo", "steps": 2 }
    ]
  }
]
*/