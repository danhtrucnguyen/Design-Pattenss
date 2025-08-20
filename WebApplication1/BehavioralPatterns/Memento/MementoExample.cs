using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Memento
{
    // ===== DTOs cho Postman =====
    public sealed class ActDto
    {
        // "add" | "remove" | "coupon" | "clear" | "save" | "undo" | "redo"
        public string Type { get; set; } = "add";
        public string? Sku { get; set; }
        public int? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Percent { get; set; }
        public string? Code { get; set; }
        public string? Note { get; set; }
    }

    public sealed class MementoJobDto
    {
        public string? Title { get; set; }
        public List<ActDto> Actions { get; set; } = new();
    }

    public static class MementoExample
    {
        public static object Run(List<MementoJobDto> jobs)
        {
            var outs = new List<object>();

            foreach (var j in jobs ?? new())
            {
                var cart = new Cart();
                var history = new CartHistory();
                var steps = new List<object>();

                foreach (var a in j.Actions ?? new())
                {
                    try
                    {
                        var t = (a.Type ?? "").Trim().ToLowerInvariant();

                        // Gợi ý: tự động Save "trước khi thay đổi" (nếu là lệnh mutate)
                        bool isMutate = t is "add" or "remove" or "coupon" or "clear";
                        if (isMutate)
                            history.Save(cart, $"before {t}");

                        switch (t)
                        {
                            case "add":
                                cart.Add(Req(a.Sku, "sku"), a.Qty ?? 0, a.UnitPrice ?? 0m);
                                steps.Add(new { done = "add", a.Sku, a.Qty, a.UnitPrice, snapshot = View(cart) });
                                break;

                            case "remove":
                                cart.Remove(Req(a.Sku, "sku"), a.Qty ?? 0);
                                steps.Add(new { done = "remove", a.Sku, a.Qty, snapshot = View(cart) });
                                break;

                            case "coupon":
                                cart.ApplyCoupon(a.Percent ?? 0m, a.Code ?? "COUPON");
                                steps.Add(new { done = "coupon", a.Code, a.Percent, snapshot = View(cart) });
                                break;

                            case "clear":
                                cart.ClearCoupon();
                                steps.Add(new { done = "clear", snapshot = View(cart) });
                                break;

                            case "save":
                                history.Save(cart, a.Note ?? "manual save");
                                steps.Add(new { done = "save", note = a.Note, stacks = history.ViewStacks() });
                                break;

                            case "undo":
                                {
                                    var m = history.Undo(cart);
                                    steps.Add(new { done = "undo", to = m?.Note, snapshot = View(cart), stacks = history.ViewStacks() });
                                    break;
                                }
                            case "redo":
                                {
                                    var m = history.Redo(cart);
                                    steps.Add(new { done = "redo", to = m?.Note, snapshot = View(cart), stacks = history.ViewStacks() });
                                    break;
                                }

                            default:
                                steps.Add(new { error = $"unsupported action: {a.Type}" });
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        steps.Add(new { action = a.Type, error = ex.Message });
                    }
                }

                outs.Add(new { title = j.Title, final = View(cart), stacks = history.ViewStacks(), steps });
            }

            return new { count = outs.Count, results = outs };
        }

        private static object View(Cart c) => new
        {
            items = c.Lines.Select(kv => new { sku = kv.Key, quantity = kv.Value.qty, unitPrice = kv.Value.price, lineTotal = kv.Value.qty * kv.Value.price }),
            subtotal = c.Subtotal,
            discountPercent = c.DiscountPercent,
            discount = c.Discount,
            coupon = c.CouponCode,
            total = c.Total
        };

        private static string Req(string? s, string field)
            => string.IsNullOrWhiteSpace(s) ? throw new ArgumentException($"{field} required") : s.Trim();
    }
}

//GET http://localhost:5102/api/memento/demo
//POST http://localhost:5102/api/memento/run

/*
[
  {
    "title": "Thử nghiệm coupon & undo/redo",
    "actions": [
      { "type": "add",    "sku": "A001", "qty": 2, "unitPrice": 50 },
      { "type": "add",    "sku": "B002", "qty": 1, "unitPrice": 200 },
      { "type": "save",   "note": "Điểm A" },
      { "type": "coupon", "code": "SALE10", "percent": 0.10 },
      { "type": "save",   "note": "Điểm B" },
      { "type": "remove", "sku": "A001", "qty": 1 },
      { "type": "undo" },
      { "type": "undo" },
      { "type": "redo" }
    ]
  }
]
*/