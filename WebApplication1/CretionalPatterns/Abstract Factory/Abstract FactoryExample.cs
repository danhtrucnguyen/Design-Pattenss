using System;
using System.Collections.Generic;
using WebApplication1.CretionalPatterns.Abstract_Factory;

namespace WebApplication1.CretionalPatterns.AbstractFactory
{
    // DTO cho Postman
    public sealed class PaymentJobDto
    {
        // "visa" | "paypal"
        public string Provider { get; set; } = "visa";
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public static class AbstractFactoryExample
    {
        public static object Run(List<PaymentJobDto> jobs)
        {
            var results = new List<object>();

            foreach (var j in jobs)
            {
                try
                {
                    var factory = CreateFactory(j.Provider);
                    var service = new CheckoutService(factory);

                    var (blocked, result, receipt) = service.Process(new PaymentRequest(j.Amount, j.Currency));

                    results.Add(new
                    {
                        input = j,
                        fraudBlocked = blocked,
                        result = result is null ? null : new { result.Success, result.Provider, result.Message },
                        receipt
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new { input = j, error = ex.Message });
                }
            }

            return new { count = results.Count, results };
        }

        private static IPaymentSuiteFactory CreateFactory(string provider)
        {
            return (provider ?? "").Trim().ToLowerInvariant() switch
            {
                "visa" => new VisaSuiteFactory(),
                "paypal" => new PaypalSuiteFactory(),
                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };
        }
    }
}
