

using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.StructuralPatterns.Composite
{
    public interface ICartComponent
    {
        decimal GetPrice();
    }

    // Leaf
    public sealed record CartItem(string Sku, int Quantity, decimal UnitPrice) : ICartComponent
    {
        public decimal GetPrice()
        {
            if (Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(Quantity));
            if (UnitPrice < 0) throw new ArgumentOutOfRangeException(nameof(UnitPrice));
            return Quantity * UnitPrice;
        }
    }

    // Composite
    public sealed class CartBundle : ICartComponent
    {
        private readonly List<ICartComponent> _children = new();

        public string Name { get; }
        public decimal DiscountAmount { get; }

        public CartBundle(string name, decimal discountAmount = 0m)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Bundle name is required");
            if (discountAmount < 0) throw new ArgumentOutOfRangeException(nameof(discountAmount));
            Name = name.Trim();
            DiscountAmount = discountAmount;
        }

        public CartBundle Add(ICartComponent child)
        {
            _children.Add(child ?? throw new ArgumentNullException(nameof(child)));
            return this;
        }

        public IReadOnlyList<ICartComponent> Children => _children.AsReadOnly();

        public decimal GetPrice()
        {
            var sum = _children.Sum(c => c.GetPrice());
            return Math.Max(0, sum - DiscountAmount);
        }
    }
}

/*[
    {
        "name": "Combo 1",
        "discountAmount": 20,
        "items": [
            { "sku": "A001", "quantity": 2, "unitPrice": 50 },
            { "sku": "B002", "quantity": 1, "unitPrice": 200 }
        ]
    },
    {
    "name": "Combo 2",
        "discountAmount": 15,
        "items": [
            { "sku": "C003", "quantity": 3, "unitPrice": 30 }
        ]
    }
]*/
