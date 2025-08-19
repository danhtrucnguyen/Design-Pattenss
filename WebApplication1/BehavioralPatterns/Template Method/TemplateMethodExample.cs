using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.TemplateMethod
{
    // ===== DTO cho Postman =====
    public sealed class BeverageJobDto
    {
        // "coffee" | "tea"
        public string Type { get; set; } = "coffee";
        public string? Title { get; set; }

        // Hook tuỳ chọn
        public int? WaterTempC { get; set; }        // ví dụ: 95 cho trà
        public bool? WithCondiments { get; set; }   // true/false để bật/tắt gia vị
    }

    public static class TemplateExample
    {
        public static object Run(List<BeverageJobDto> jobs)
        {
            var outputs = new List<object>();

            foreach (var j in jobs ?? new())
            {
                try
                {
                    var maker = Make(j.Type, j.WaterTempC, j.WithCondiments);
                    var steps = maker.Prepare();

                    outputs.Add(new
                    {
                        title = j.Title,
                        type = j.Type,
                        waterTempC = j.WaterTempC,
                        withCondiments = j.WithCondiments,
                        steps
                    });
                }
                catch (Exception ex)
                {
                    outputs.Add(new { title = j.Title, type = j.Type, error = ex.Message });
                }
            }

            return new { count = outputs.Count, results = outputs };
        }

        private static BeverageMaker Make(string type, int? temp, bool? condiments)
            => (type ?? "coffee").Trim().ToLowerInvariant() switch
            {
                "coffee" => new CoffeeMaker(temp, condiments),
                "tea" => new TeaMaker(temp, condiments),
                _ => throw new ArgumentException($"Loại đồ uống không hỗ trợ: {type}")
            };
    }
}
