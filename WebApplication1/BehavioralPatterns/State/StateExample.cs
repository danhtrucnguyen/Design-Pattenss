using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.State
{
    // ===== DTOs =====
    public sealed class ActionDto
    {
        // "add" | "pay" | "ship" | "deliver" | "cancel"
        public string Type { get; set; } = "add";
        public decimal? Amount { get; set; }
        public int? Qty { get; set; }
    }

    public sealed class StateJobDto
    {
        public string? Title { get; set; }
        public List<ActionDto> Actions { get; set; } = new();
    }

    // ===== Runner =====
    public static class StateExample
    {
        public static object Run(List<StateJobDto> jobs)
        {
            var outs = new List<object>();
            foreach (var j in jobs ?? new())
            {
                var order = new Order(id: "ORD-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant());
                var steps = new List<object>();

                foreach (var a in j.Actions ?? new())
                {
                    var log = new List<string>();
                    switch ((a.Type ?? "").Trim().ToLowerInvariant())
                    {
                        case "add": order.Add(a.Amount ?? 0m, a.Qty ?? 0, log); break;
                        case "pay": order.Pay(log); break;
                        case "ship": order.Ship(log); break;
                        case "deliver": order.Deliver(log); break;
                        case "cancel": order.Cancel(log); break;
                        default: log.Add($"Unsupported action: {a.Type}"); break;
                    }
                    steps.Add(new
                    {
                        action = a.Type,
                        state = order.StateName,
                        subtotal = order.Subtotal,
                        log
                    });
                }

                outs.Add(new
                {
                    title = j.Title,
                    orderId = order.Id,
                    final = new { state = order.StateName, subtotal = order.Subtotal },
                    steps
                });
            }
            return new { count = outs.Count, results = outs };
        }
    }
}
