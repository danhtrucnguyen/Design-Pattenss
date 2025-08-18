using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
namespace WebApplication1.CretionalPatterns.Singleton
{
    public sealed class LogJobDto
    {
        public List<string> Messages { get; set; } = new();
        public string? Prefix { get; set; } // tuỳ chọn: thêm tiền tố vào từng log
    }

    public static class SingletonExample
    {
        public static object Run(List<LogJobDto> jobs)
        {
            var results = new List<object>();
            var loggerA = LoggerSingleton.Instance;
            var loggerB = LoggerSingleton.Instance;

            // xác nhận Singleton
            bool same = ReferenceEquals(loggerA, loggerB);
            int instanceId = RuntimeHelpers.GetHashCode(loggerA);

            foreach (var job in jobs)
            {
                var before = loggerA.Snapshot().Length;

                foreach (var msg in job.Messages ?? Enumerable.Empty<string>())
                {
                    var line = string.IsNullOrWhiteSpace(job.Prefix) ? msg : $"[{job.Prefix}] {msg}";
                    loggerA.Log(line);
                }

                var after = loggerA.Snapshot().Length;
                results.Add(new
                {
                    wrote = (after - before),
                    countBefore = before,
                    countAfter = after
                });
            }

            // trả về snapshot (giới hạn 100 dòng cuối để tránh quá dài)
            var logs = loggerA.Snapshot();
            var last = logs.Length > 100 ? logs[^100..] : logs;

            return new
            {
                singleton = new
                {
                    sameInstance = same,
                    instanceId
                },
                jobs = results,
                totalLogs = logs.Length,
                lastLogs = last
            };
        }
    }
}
