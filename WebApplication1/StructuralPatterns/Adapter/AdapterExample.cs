using WebApplication1.StructuralPatterns.Adapter;

namespace Design_Patterns.StructuralPatterns.Adapter
{
    // DTO nhận từ Postman
    public sealed class PaymentJobDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Provider { get; set; } = "oldpay"; // demo 1 gateway
    }

    public static class AdapterExample
    {
        // Map DTO -> Domain, chạy qua Adapter, trả JSON chuẩn hoá
        public static object Run(List<PaymentJobDto> jobs)
        {
            var results = new List<object>();

            // Hiện tại demo 1 provider "oldpay"
            var legacy = new OldPayGateway();
            var processor = new GatewayAdapter("OldPay", legacy);

            foreach (var j in jobs)
            {
                if (!string.Equals(j.Provider, "oldpay", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new
                    {
                        provider = j.Provider,
                        success = false,
                        message = "Unsupported provider",
                        amount = j.Amount,
                        currency = j.Currency
                    });
                    continue;
                }

                var req = new PaymentRequest(j.Amount, j.Currency);
                var res = processor.Process(req);

                results.Add(new
                {
                    provider = res.Provider,
                    success = res.Success,
                    message = res.Message
                });
            }

            return new
            {
                count = jobs.Count,
                results
            };
        }
    }
}
