using System;

namespace WebApplication1.StructuralPatterns.Adapter
{
    public record PaymentRequest(decimal Amount, string Currency);
    public record PaymentResult(bool Success, string Provider, string Message);

    public interface IPaymentProcessor
    {
        string Name { get; }
        PaymentResult Process(PaymentRequest req);
    }

    // Adaptee
    public record LegacyResponse(int Code, string Text);

    public interface ILegacyPaymentGateway
    {
        LegacyResponse Pay(int amountInCents, string currencyCode);
    }

    public sealed class OldPayGateway : ILegacyPaymentGateway
    {
        public LegacyResponse Pay(int amountInCents, string currencyCode)
        {
            if (amountInCents <= 0) return new LegacyResponse(400, "invalid_amount");
            if (string.IsNullOrWhiteSpace(currencyCode)) return new LegacyResponse(422, "invalid_currency");
            return new LegacyResponse(200, $"ok:{amountInCents}:{currencyCode.ToUpperInvariant()}");
        }
    }

    // Adapter
    public sealed class GatewayAdapter : IPaymentProcessor
    {
        private readonly ILegacyPaymentGateway _gateway;
        public string Name { get; }

        public GatewayAdapter(string providerName, ILegacyPaymentGateway gateway)
        {
            Name = providerName ?? "LEGACY";
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        public PaymentResult Process(PaymentRequest req)
        {
            var cents = checked((int)Math.Round(req.Amount * 100m, MidpointRounding.AwayFromZero));
            var code = NormalizeCurrency(req.Currency);

            var resp = _gateway.Pay(cents, code);
            return Map(resp);
        }

        private static string NormalizeCurrency(string c)
            => string.IsNullOrWhiteSpace(c) ? "USD" : c.Trim().ToUpperInvariant();

        private PaymentResult Map(LegacyResponse r) => r.Code switch
        {
            200 => new(true, Name, $"Charged via {Name}: {r.Text}"),
            400 => new(false, Name, "Amount is invalid"),
            401 => new(false, Name, "Unauthorized"),
            422 => new(false, Name, "Currency is invalid"),
            500 => new(false, Name, "Gateway error"),
            _ => new(false, Name, $"Unknown error ({r.Code}): {r.Text}")
        };
    }
}

//http://localhost:5102/api/adapter/demo

//http://localhost:5102/api/adapter/pay

/*
[
  { "amount": 50.75, "currency": "usd", "provider": "oldpay" },
  { "amount": 0,      "currency": "usd", "provider": "oldpay" },
  { "amount": 20,     "currency": "",    "provider": "oldpay" },
  { "amount": 10,     "currency": "usd", "provider": "other" }
]
*/