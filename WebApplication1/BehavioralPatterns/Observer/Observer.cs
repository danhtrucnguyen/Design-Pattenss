using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.Observer
{
    //Subject Event
    public sealed record ProductEvent(string Kind, decimal OldValue, decimal NewValue); // Kind: "price" | "stock"

    //Observer contract
    public interface IProductObserver
    {
        string Name { get; }
        string? OnChanged(ProductSubject product, ProductEvent ev);
    }

    //Subject
    public sealed class ProductSubject
    {
        public string Sku { get; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }

        private readonly List<IProductObserver> _observers = new();

        public ProductSubject(string sku, decimal price, int stock)
        {
            Sku = sku ?? throw new ArgumentNullException(nameof(sku));
            Price = price;
            Stock = stock;
        }

        public void Attach(IProductObserver obs) { if (obs != null) _observers.Add(obs); }
        public void Detach(IProductObserver obs) { _observers.Remove(obs); }

        public IReadOnlyList<string> SetPrice(decimal newPrice)
        {
            var ev = new ProductEvent("price", Price, newPrice);
            Price = newPrice;
            return Notify(ev);
        }

        public IReadOnlyList<string> SetStock(int newStock)
        {
            var ev = new ProductEvent("stock", Stock, newStock);
            Stock = newStock;
            return Notify(ev);
        }

        private List<string> Notify(ProductEvent ev)
        {
            var msgs = new List<string>();
            foreach (var obs in _observers)
            {
                var m = obs.OnChanged(this, ev);
                if (!string.IsNullOrWhiteSpace(m)) msgs.Add(m!);
            }
            return msgs;
        }
    }

    //Concrete Observers
    public sealed class PriceDropObserver : IProductObserver
    {
        public string Name { get; }
        private readonly decimal _target;
        public PriceDropObserver(string name, decimal target) { Name = name; _target = target; }

        public string? OnChanged(ProductSubject p, ProductEvent ev)
        {
            if (ev.Kind != "price") return null;
            // chỉ báo khi giảm xuống bằng/nhỏ hơn target và có giảm thật (old > new)
            if (ev.OldValue > ev.NewValue && p.Price <= _target)
                return $"[{Name}] Deal alert {p.Sku}: {p.Price} ≤ target {_target}";
            return null;
        }
    }

    public sealed class LowStockObserver : IProductObserver
    {
        public string Name { get; }
        private readonly int _threshold;
        public LowStockObserver(string name, int threshold) { Name = name; _threshold = threshold; }

        public string? OnChanged(ProductSubject p, ProductEvent ev)
        {
            if (ev.Kind != "stock") return null;
            if (p.Stock <= _threshold)
                return $"[{Name}] Low stock {p.Sku}: {p.Stock} ≤ {_threshold}";
            return null;
        }
    }
}

//GET http://localhost:5102/api/observer/demo
//POST http://localhost:5102/api/observer/run

/*
[
  {
    "title": "Track price & stock",
    "product": { "sku": "NB-001", "price": 1500, "stock": 5 },
    "watchers": [
      { "type": "price", "name": "Alice",  "targetPrice": 1200 },
      { "type": "stock", "name": "OpsBot", "threshold": 2 }
    ],
    "actions": [
      { "type": "price", "newValue": 1299 },
      { "type": "stock", "newValue": 2 },
      { "type": "price", "newValue": 1199 },
      { "type": "stock", "newValue": 1 }
    ]
  }
]
*/