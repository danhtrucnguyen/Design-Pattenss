using System;

namespace WebApplication1.CretionalPatterns.FactoryMethod
{
    public record PaymentRequest(decimal Amount, string Currency);
    public record PaymentResult(bool Success, string Provider, string Message);

    // Product
    public interface IPaymentProcessor
    {
        string Name { get; }
        PaymentResult Process(PaymentRequest req);
    }

    // Concrete Products
    public sealed class VisaProcessor : IPaymentProcessor
    {
        public string Name => "VISA";
        public PaymentResult Process(PaymentRequest req)
        {
            if (req.Amount <= 0) return new(false, Name, "So tien khong hop le");
            return new(true, Name, $"Da tinh phi {req.Amount} {req.Currency} qua VISA");
        }
    }

    public sealed class PaypalProcessor : IPaymentProcessor
    {
        public string Name => "PAYPAL";
        public PaymentResult Process(PaymentRequest req)
        {
            if (req.Amount <= 0) return new(false, Name, "So tien khong hop le");
            return new(true, Name, $"Da tinh phi {req.Amount} {req.Currency} qua PayPal");
        }
    }

    public sealed class MomoProcessor : IPaymentProcessor
    {
        public string Name => "MOMO";
        public PaymentResult Process(PaymentRequest req)
        {
            if (req.Amount <= 0) return new(false, Name, "So tien khong hop le");
            return new(true, Name, $"Da tinh phi {req.Amount} {req.Currency} qua MoMo");
        }
    }

    // Creator
    public abstract class CheckoutPayment
    {
        public PaymentResult Pay(PaymentRequest req)
        {
            var processor = CreateProcessor();   // <— Factory Method
            return processor.Process(req);
        }

        protected abstract IPaymentProcessor CreateProcessor();
    }

    // Concrete Creators
    public sealed class VisaCheckout : CheckoutPayment { protected override IPaymentProcessor CreateProcessor() => new VisaProcessor(); }
    public sealed class PaypalCheckout : CheckoutPayment { protected override IPaymentProcessor CreateProcessor() => new PaypalProcessor(); }
    public sealed class MomoCheckout : CheckoutPayment { protected override IPaymentProcessor CreateProcessor() => new MomoProcessor(); }
}
