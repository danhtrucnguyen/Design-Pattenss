using Microsoft.Extensions.Hosting;
using System;

namespace WebApplication1.CretionalPatterns.Abstract_Factory
{
    // Contracts
    public record PaymentRequest(decimal Amount, string Currency);
    public record PaymentResult(bool Success, string Provider, string Message);

    public interface IPaymentProcessor
    {
        string Name { get; }
        PaymentResult Process(PaymentRequest req);
    }

    public interface IFraudChecker
    {
        bool IsSuspicious(PaymentRequest req);
    }

    public interface IReceiptFormatter
    {
        string Format(PaymentResult res);
    }

    public interface IPaymentSuiteFactory
    {
        IPaymentProcessor CreateProcessor();
        IFraudChecker CreateFraudChecker();
        IReceiptFormatter CreateReceiptFormatter();
    }

    // ===== VISA concrete products =====
    public sealed class VisaProcessor : IPaymentProcessor
    {
        public string Name => "VISA";
        public PaymentResult Process(PaymentRequest req)
        {
            if (req.Amount <= 0) return new(false, Name, "Don hang khong hop le");
            return new(true, Name, $"Da tinh phi {req.Amount} {req.Currency} qua VISA");
        }
    }

    public sealed class VisaFraudChecker : IFraudChecker
    {
        public bool IsSuspicious(PaymentRequest req) => req.Amount > 10_000m; // demo rule
    }

    public sealed class VisaReceiptFormatter : IReceiptFormatter
    {
        public string Format(PaymentResult res) =>
            res.Success ? $"[VISA RECEIPT] {res.Message}" : $"[VISA FAIL] {res.Message}";
    }

    public sealed class VisaSuiteFactory : IPaymentSuiteFactory
    {
        public IPaymentProcessor CreateProcessor() => new VisaProcessor();
        public IFraudChecker CreateFraudChecker() => new VisaFraudChecker();
        public IReceiptFormatter CreateReceiptFormatter() => new VisaReceiptFormatter();
    }

    // ===== PayPal concrete products =====
    public sealed class PaypalProcessor : IPaymentProcessor
    {
        public string Name => "PAYPAL";
        public PaymentResult Process(PaymentRequest req)
        {
            if (req.Amount <= 0) return new(false, Name, "Don hang khong hop le");
            return new(true, Name, $"Da tinh phi {req.Amount} {req.Currency} qua PayPal");
        }
    }

    public sealed class PaypalFraudChecker : IFraudChecker
    {
        public bool IsSuspicious(PaymentRequest req) => req.Amount > 8_000m;
    }

    public sealed class PaypalReceiptFormatter : IReceiptFormatter
    {
        public string Format(PaymentResult res) =>
            res.Success ? $"<PAYPAL RECEIPT> {res.Message}" : $"<PAYPAL FAIL> {res.Message}";
    }

    public sealed class PaypalSuiteFactory : IPaymentSuiteFactory
    {
        public IPaymentProcessor CreateProcessor() => new PaypalProcessor();
        public IFraudChecker CreateFraudChecker() => new PaypalFraudChecker();
        public IReceiptFormatter CreateReceiptFormatter() => new PaypalReceiptFormatter();
    }

    // ===== Client =====
    public sealed class CheckoutService
    {
        private readonly IPaymentSuiteFactory _factory;
        public CheckoutService(IPaymentSuiteFactory factory) => _factory = factory;

        // API-friendly detail: trả về đủ thông tin
        public (bool FraudBlocked, PaymentResult? Result, string Receipt) Process(PaymentRequest req)
        {
            var fraud = _factory.CreateFraudChecker();
            if (fraud.IsSuspicious(req))
                return (true, null, "GIAO DICH KHONG THANH CONG: Thanh toan vuot muc gioi han");

            var processor = _factory.CreateProcessor();
            var result = processor.Process(req);

            var formatter = _factory.CreateReceiptFormatter();
            var receipt = formatter.Format(result);

            return (false, result, receipt);
        }

        
        public string PayAndPrintReceipt(PaymentRequest req)
        {
            var (blocked, res, receipt) = Process(req);
            return blocked ? receipt : receipt;
        }
    }
}

//GET http://localhost:5102/api/abstractfactory/demo
//POST http://localhost:5102/api/abstractfactory/pay

/*
[
  { "provider": "visa",   "amount": 5000,  "currency": "USD" },
  { "provider": "paypal", "amount": 9000,  "currency": "USD" },
  { "provider": "visa",   "amount": 15000, "currency": "USD" }
]
*/
