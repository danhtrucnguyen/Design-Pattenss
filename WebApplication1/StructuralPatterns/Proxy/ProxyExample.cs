using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace WebApplication1.StructuralPatterns.Proxy
{
    // ===== DTOs =====
    public sealed class InventoryCommandDto
    {
        // "get" | "add" | "reserve" | "wait"
        public string Action { get; set; } = "get";
        public string? Sku { get; set; }
        public int Qty { get; set; } = 0;
        public int? DelayMs { get; set; } // chỉ dùng cho action "wait"
    }

    public sealed class InventoryJobDto
    {
        public Dictionary<string, int> InitialStock { get; set; } = new();
        public int LatencyMs { get; set; } = 25;

        public bool UseLazy { get; set; } = true;
        public bool UseCaching { get; set; } = true;
        public int TtlMs { get; set; } = 1500;
        public bool UseProtection { get; set; } = false;
        public bool CanWrite { get; set; } = true;

        public List<InventoryCommandDto> Commands { get; set; } = new();
    }

    public static class ProxyExample
    {
        public static object Run(List<InventoryJobDto> jobs)
        {
            var results = new List<object>();

            foreach (var j in jobs)
            {
                // --- compose proxies theo options ---
                RealInventory? real = null;
                IInventory current;

                if (j.UseLazy)
                {
                    var lazy = new LazyInventoryProxy(() =>
                    {
                        real = new RealInventory(j.InitialStock, j.LatencyMs);
                        return real;
                    });
                    current = lazy;
                }
                else
                {
                    real = new RealInventory(j.InitialStock, j.LatencyMs);
                    current = real;
                }

                if (j.UseProtection)
                    current = new ProtectionInventoryProxy(current, () => j.CanWrite);

                if (j.UseCaching)
                    current = new CachingInventoryProxy(current, TimeSpan.FromMilliseconds(j.TtlMs));

                // --- chạy commands ---
                var logs = new List<object>();
                var skusTouched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var c in j.Commands)
                {
                    var act = (c.Action ?? "").Trim().ToLowerInvariant();
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        switch (act)
                        {
                            case "wait":
                                Thread.Sleep(c.DelayMs.GetValueOrDefault(0));
                                sw.Stop();
                                logs.Add(new { action = "wait", ms = sw.ElapsedMilliseconds });
                                break;

                            case "get":
                                if (string.IsNullOrWhiteSpace(c.Sku)) throw new ArgumentException("SKU is required for GET");
                                var v = current.GetStock(c.Sku);
                                skusTouched.Add(c.Sku);
                                sw.Stop();
                                logs.Add(new { action = "get", sku = c.Sku, value = v, ms = sw.ElapsedMilliseconds });
                                break;

                            case "add":
                                if (string.IsNullOrWhiteSpace(c.Sku)) throw new ArgumentException("SKU is required for ADD");
                                current.AddStock(c.Sku, c.Qty);
                                skusTouched.Add(c.Sku);
                                sw.Stop();
                                logs.Add(new { action = "add", sku = c.Sku, qty = c.Qty, ok = true, ms = sw.ElapsedMilliseconds });
                                break;

                            case "reserve":
                                if (string.IsNullOrWhiteSpace(c.Sku)) throw new ArgumentException("SKU is required for RESERVE");
                                var ok = current.Reserve(c.Sku, c.Qty);
                                skusTouched.Add(c.Sku);
                                sw.Stop();
                                logs.Add(new { action = "reserve", sku = c.Sku, qty = c.Qty, ok, ms = sw.ElapsedMilliseconds });
                                break;

                            default:
                                sw.Stop();
                                logs.Add(new { error = $"Unsupported action '{c.Action}'" });
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        logs.Add(new { action = act, error = ex.Message, ms = sw.ElapsedMilliseconds });
                    }
                }

                // đọc số lần GetStock của RealInventory (nếu đã khởi tạo)
                int? realGetStockCalls = real?.GetStockCalls;

                // tổng kết stock hiện tại của các SKU đã đụng tới
                var finalStock = skusTouched
                    .ToDictionary(s => s, s => current.GetStock(s), StringComparer.OrdinalIgnoreCase);

                results.Add(new
                {
                    options = new
                    {
                        j.UseLazy,
                        j.UseCaching,
                        j.TtlMs,
                        j.UseProtection,
                        j.CanWrite,
                        j.LatencyMs
                    },
                    initialStock = j.InitialStock,
                    logs,
                    finalStock,
                    realGetStockCalls
                });
            }

            return new { count = jobs.Count, results };
        }
    }
}
