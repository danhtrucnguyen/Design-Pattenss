using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Memento
{
    // Originator: Cart — có Save() tạo snapshot và Restore() để khôi phục.
    // Memento:    CartSnapshot — trạng thái bất biến của Cart (items, coupon, discount).
    // Caretaker:  CartHistory — giữ stack undo/redo, không can thiệp chi tiết Cart.

    //Originator
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
            return take;
        }

        public void ApplyCoupon(decimal percent, string code)
        {
            DiscountPercent = Math.Clamp(percent, 0m, 1m);
            CouponCode = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        }

        public void ClearCoupon()
        {
            DiscountPercent = 0m;
            CouponCode = null;
        }

        public IReadOnlyDictionary<string, (int qty, decimal price)> Lines => _lines;
        public decimal DiscountPercent { get; private set; }
        public string? CouponCode { get; private set; }

        public decimal Subtotal => _lines.Sum(kv => kv.Value.qty * kv.Value.price);
        public decimal Discount => Math.Round(Subtotal * DiscountPercent, 2, MidpointRounding.AwayFromZero);
        public decimal Total => Subtotal - Discount;

        //Memento API
        public ICartMemento Save(string note = "")
        {
            var items = _lines.Select(kv => new CartLine(kv.Key, kv.Value.qty, kv.Value.price)).ToList();
            return new CartSnapshot(items, DiscountPercent, CouponCode, note, DateTime.UtcNow);
        }

        public void Restore(ICartMemento m)
        {
            if (m is not CartSnapshot s) throw new ArgumentException("Invalid memento");
            _lines.Clear();
            foreach (var it in s.Items)
                _lines[it.Sku] = (it.Qty, it.UnitPrice);
            DiscountPercent = s.DiscountPercent;
            CouponCode = s.CouponCode;
        }
    }

    //Memento
    public interface ICartMemento
    {
        string Note { get; }
        DateTime At { get; }
    }

    public sealed record CartLine(string Sku, int Qty, decimal UnitPrice);

    public sealed record CartSnapshot(
        IReadOnlyList<CartLine> Items,
        decimal DiscountPercent,
        string? CouponCode,
        string Note,
        DateTime At
    ) : ICartMemento;

    //Caretaker
    public sealed class CartHistory
    {
        private readonly Stack<ICartMemento> _undo = new();
        private readonly Stack<ICartMemento> _redo = new();

        public void Save(Cart cart, string note)
        {
            _undo.Push(cart.Save(note));
            _redo.Clear(); // mỗi lần lưu trạng thái mới thì xóa redo branch
        }

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public ICartMemento? Undo(Cart cart)
        {
            if (_undo.Count == 0) return null;
            var snap = _undo.Pop();
            _redo.Push(cart.Save("[auto-redo]")); // ghi lại trạng thái hiện tại để có thể Redo
            cart.Restore(snap);
            return snap;
        }

        public ICartMemento? Redo(Cart cart)
        {
            if (_redo.Count == 0) return null;
            var snap = _redo.Pop();
            _undo.Push(cart.Save("[auto-undo]"));
            cart.Restore(snap);
            return snap;
        }

        public object ViewStacks() => new
        {
            undo = _undo.Select(m => new { m.Note, At = m.At }).ToList(),
            redo = _redo.Select(m => new { m.Note, At = m.At }).ToList()
        };
    }
}
