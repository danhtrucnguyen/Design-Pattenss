using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.State
{
    //Context
    public sealed class Order
    {
        private IOrderState _state;
        public string Id { get; }
        public decimal Subtotal { get; private set; }
        public string StateName => _state.Name;

        public Order(string id)
        {
            Id = id ?? Guid.NewGuid().ToString("N");
            _state = new CartState(); 
        }

        public void Add(decimal amount, int qty, List<string> log) => _state.Add(this, amount, qty, log);
        public void Pay(List<string> log) => _state.Pay(this, log);
        public void Ship(List<string> log) => _state.Ship(this, log);
        public void Deliver(List<string> log) => _state.Deliver(this, log);
        public void Cancel(List<string> log) => _state.Cancel(this, log);

        // APIs cho State dùng
        internal void Increase(decimal amount, int qty) => Subtotal += Math.Max(0, amount) * Math.Max(0, qty);
        internal void SetState(IOrderState s) => _state = s ?? _state;
    }

    //State contract
    public interface IOrderState
    {
        string Name { get; }
        void Add(Order o, decimal amount, int qty, List<string> log);
        void Pay(Order o, List<string> log);
        void Ship(Order o, List<string> log);
        void Deliver(Order o, List<string> log);
        void Cancel(Order o, List<string> log);
    }

    //Concrete States
    public sealed class CartState : IOrderState
    {
        public string Name => "Cart";
        public void Add(Order o, decimal amount, int qty, List<string> log)
        {
            if (amount <= 0 || qty <= 0) { log.Add("Add bị từ chối: amount/qty phải > 0"); return; }
            o.Increase(amount, qty);
            log.Add($"Đã thêm {qty} món x {amount} → Subtotal={o.Subtotal}");
        }
        public void Pay(Order o, List<string> log)
        {
            if (o.Subtotal <= 0) { log.Add("Pay thất bại: giỏ hàng trống"); return; }
            o.SetState(new PaidState());
            log.Add("Thanh toán thành công → state=Paid");
        }
        public void Ship(Order o, List<string> log) => log.Add("Không thể Ship khi còn ở Cart");
        public void Deliver(Order o, List<string> log) => log.Add("Không thể Deliver khi còn ở Cart");
        public void Cancel(Order o, List<string> log)
        {
            o.SetState(new CancelledState());
            log.Add("Đã hủy đơn → state=Cancelled");
        }
    }

    public sealed class PaidState : IOrderState
    {
        public string Name => "Paid";
        public void Add(Order o, decimal amount, int qty, List<string> log)
            => log.Add("Không thể thêm hàng sau khi đã thanh toán");
        public void Pay(Order o, List<string> log) => log.Add("Đơn đã được thanh toán");
        public void Ship(Order o, List<string> log)
        {
            o.SetState(new ShippedState());
            log.Add("Đã tạo vận đơn → state=Shipped");
        }
        public void Deliver(Order o, List<string> log) => log.Add("Chưa thể Deliver khi chưa Ship");
        public void Cancel(Order o, List<string> log)
        {
            o.SetState(new CancelledState());
            log.Add("Đã hoàn tiền & hủy đơn → state=Cancelled");
        }
    }

    public sealed class ShippedState : IOrderState
    {
        public string Name => "Shipped";
        public void Add(Order o, decimal amount, int qty, List<string> log)
            => log.Add("Không thể thêm hàng sau khi đã Ship");
        public void Pay(Order o, List<string> log) => log.Add("Đơn đã thanh toán");
        public void Ship(Order o, List<string> log) => log.Add("Đã Ship rồi");
        public void Deliver(Order o, List<string> log)
        {
            o.SetState(new DeliveredState());
            log.Add("Giao hàng thành công → state=Delivered");
        }
        public void Cancel(Order o, List<string> log) => log.Add("Không thể hủy sau khi đã Ship");
    }

    public sealed class DeliveredState : IOrderState
    {
        public string Name => "Delivered";
        public void Add(Order o, decimal amount, int qty, List<string> log) => log.Add("Đơn đã giao xong");
        public void Pay(Order o, List<string> log) => log.Add("Đơn đã giao xong");
        public void Ship(Order o, List<string> log) => log.Add("Đơn đã giao xong");
        public void Deliver(Order o, List<string> log) => log.Add("Đơn đã giao xong");
        public void Cancel(Order o, List<string> log) => log.Add("Không thể hủy: đơn đã giao xong");
    }

    public sealed class CancelledState : IOrderState
    {
        public string Name => "Cancelled";
        public void Add(Order o, decimal amount, int qty, List<string> log) => log.Add("Đơn đã hủy");
        public void Pay(Order o, List<string> log) => log.Add("Đơn đã hủy");
        public void Ship(Order o, List<string> log) => log.Add("Đơn đã hủy");
        public void Deliver(Order o, List<string> log) => log.Add("Đơn đã hủy");
        public void Cancel(Order o, List<string> log) => log.Add("Đơn đã hủy");
    }
}


//GET http://localhost:5102/api/state/demo
//POST http://localhost:5102/api/state/run

/*
[
  {
    "title": "Thanh toán rồi ship",
    "actions": [
      { "type": "add", "amount": 50, "qty": 2 },
      { "type": "add", "amount": 200, "qty": 1 },
      { "type": "pay" },
      { "type": "ship" },
      { "type": "deliver" }
    ]
  }
]
*/