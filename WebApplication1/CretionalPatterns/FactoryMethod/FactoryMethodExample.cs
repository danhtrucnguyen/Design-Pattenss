using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace WebApplication1.CretionalPatterns.FactoryMethod
{
    // DTO cho Postman
    public sealed class PaymentJobDto
    {
        // "visa" | "paypal" | "momo"
        public string Provider { get; set; } = "visa";
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public static class FactoryMethodExample
    {
        public static object Run(List<PaymentJobDto> jobs)
        {
            var results = new List<object>();

            foreach (var j in jobs)
            {
                try
                {
                    var checkout = CreateCheckout(j.Provider);
                    var res = checkout.Pay(new PaymentRequest(j.Amount, j.Currency));

                    results.Add(new
                    {
                        input = j,
                        result = new { res.Success, res.Provider, res.Message }
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        input = j,
                        error = ex.Message
                    });
                }
            }

            return new { count = results.Count, results };
        }

        private static CheckoutPayment CreateCheckout(string provider)
        {
            return (provider ?? "").Trim().ToLowerInvariant() switch
            {
                "visa" => new VisaCheckout(),
                "paypal" => new PaypalCheckout(),
                "momo" => new MomoCheckout(),
                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };
        }
    }
}


//GET http://localhost:5102/api/factorymethod/demo
//POST http://localhost:5102/api/factorymethod/pay


/*
[
  { "provider": "visa",   "amount": 200,     "currency": "USD" },
  { "provider": "paypal", "amount": 500000,  "currency": "VND" },
  { "provider": "momo",   "amount": -1,      "currency": "USD" },
  { "provider": "zalo",   "amount": 10,      "currency": "VND" }
]
*/