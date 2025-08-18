using System.Collections.Concurrent;

namespace WebApplication1.CretionalPatterns.Singleton
{
    public sealed class LoggerSingleton
    {
        private static readonly Lazy<LoggerSingleton> _instance =
            new(() => new LoggerSingleton(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static LoggerSingleton Instance => _instance.Value;

        private readonly ConcurrentQueue<string> _buffer = new();

        private LoggerSingleton() { }

        public void Write(string message)
        {
            var line = $"{DateTime.UtcNow:O} [LOG] {message}";
            _buffer.Enqueue(line);
        }

        public string[] Snapshot() => _buffer.ToArray();

        // alias cho gọn
        public void Log(string message) => Write(message);
        public string[] GetLogs() => Snapshot();
    }
}
