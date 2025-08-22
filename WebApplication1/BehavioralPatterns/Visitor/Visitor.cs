using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Visitor
{
    //Visitor contracts
    public interface ICartVisitor
    {
        void Visit(PhysicalItem i);
        void Visit(DigitalItem i);
        void Visit(ServiceItem i);
    }

    public interface ICartElement
    {
        void Accept(ICartVisitor v);
        object View(); // tiện trả JSON
    }

    //Elements
    public sealed class PhysicalItem : ICartElement
    {
        public string Sku { get; }
        public int Qty { get; }
        public decimal UnitPrice { get; }
        public decimal WeightKg { get; }
        public PhysicalItem(string sku, int qty, decimal unitPrice, decimal weightKg)
        { Sku = sku; Qty = qty; UnitPrice = unitPrice; WeightKg = weightKg; }
        public void Accept(ICartVisitor v) => v.Visit(this);
        public object View() => new { type = "physical", Sku, Qty, UnitPrice, WeightKg, line = Qty * UnitPrice };
    }

    public sealed class DigitalItem : ICartElement
    {
        public string Sku { get; }
        public int Qty { get; }
        public decimal UnitPrice { get; }
        public DigitalItem(string sku, int qty, decimal unitPrice)
        { Sku = sku; Qty = qty; UnitPrice = unitPrice; }
        public void Accept(ICartVisitor v) => v.Visit(this);
        public object View() => new { type = "digital", Sku, Qty, UnitPrice, line = Qty * UnitPrice };
    }

    public sealed class ServiceItem : ICartElement
    {
        public string Sku { get; }
        public int Hours { get; }
        public decimal HourlyRate { get; }
        public ServiceItem(string sku, int hours, decimal hourlyRate)
        { Sku = sku; Hours = hours; HourlyRate = hourlyRate; }
        public void Accept(ICartVisitor v) => v.Visit(this);
        public object View() => new { type = "service", Sku, Hours, HourlyRate, line = Hours * HourlyRate };
    }

    //Visitors
    public sealed class SummaryVisitor : ICartVisitor
    {
        public decimal Subtotal { get; private set; }
        public decimal Shipping { get; private set; }
        public decimal Tax { get; private set; }
        public decimal Total => Math.Max(0, Subtotal - 0 /*discount outside*/) + Shipping + Tax;

        public void Visit(PhysicalItem i)
        {
            var line = i.Qty * i.UnitPrice;
            Subtotal += line;
            Shipping += i.Qty * 3m;                 // ship: 3/unit
            Tax += Math.Round(line * 0.10m, 2);     // VAT 10% cho hàng vật lý
        }

        public void Visit(DigitalItem i)
        {
            var line = i.Qty * i.UnitPrice;
            Subtotal += line;
            // shipping 0; tax 0 cho đơn giản
        }

        public void Visit(ServiceItem i)
        {
            var line = i.Hours * i.HourlyRate;
            Subtotal += line;
            Tax += Math.Round(line * 0.05m, 2);     // service tax 5%
        }
    }

    public sealed class DiscountVisitor : ICartVisitor
    {
        private readonly string _code;
        public decimal Discount { get; private set; }
        public string Code => _code;

        public DiscountVisitor(string? code) => _code = (code ?? "").Trim().ToUpperInvariant();

        public void Visit(PhysicalItem i)
        {
            if (_code == "SALE10")
                Discount += Math.Round(i.Qty * i.UnitPrice * 0.10m, 2);
        }

        public void Visit(DigitalItem i)
        {
            if (_code == "SALE10")
                Discount += Math.Round(i.Qty * i.UnitPrice * 0.10m, 2);
        }

        public void Visit(ServiceItem i)
        {
            if (_code == "SVC5")
                Discount += Math.Round(i.Hours * i.HourlyRate * 0.05m, 2);
        }
    }

    //Aggregate tiện ích
    public sealed class CartAggregate
    {
        private readonly List<ICartElement> _items = new();
        public void Add(ICartElement e) => _items.Add(e);
        public IEnumerable<object> View() => _items.Select(x => x.View());
        public void Accept(ICartVisitor v)
        {
            foreach (var it in _items) it.Accept(v);
        }
    }
}

//GET http://localhost:5102/api/visitor/demo
//POST http://localhost:5102/api/visitor/run

/*
[
  {
    "title": "Giỏ hàng đơn giản",
    "coupon": "SALE10",
    "items": [
      { "type": "physical", "sku": "A001", "qty": 2, "unitPrice": 50,  "weightKg": 0.5 },
      { "type": "digital",  "sku": "EBOOK1", "qty": 1, "unitPrice": 10 }
    ]
  },
  {
    "title": "Dịch vụ có giảm 5%",
    "coupon": "SVC5",
    "items": [
      { "type": "service", "sku": "INSTALL", "hours": 3, "hourlyRate": 25 }
    ]
  }
]
*/