using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services
{
    public class DebugLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, DebugLogger> _loggers = new();
        private readonly Action<LogLevel, string, string> _logAction;

        // Static storage for log messages that persists between component lifetimes
        public static readonly List<LogEntry> LogEntries = new();

        // Static delegate that can be set by the Debug component
        public static Action<LogLevel, string, string>? LogMessageAction { get; set; }

        public DebugLoggerProvider(Action<LogLevel, string, string> logAction)
        {
            _logAction = logAction;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new DebugLogger(name, _logAction));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        public static void AddLogEntry(LogLevel level, string category, string message, string formattedMessage)
        {
            LogEntries.Add(new LogEntry
            {
                Level = level,
                Category = category,
                Message = message,
                FormattedMessage = formattedMessage,
                Timestamp = DateTime.Now
            });
        }
    }

    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string Category { get; set; } = "";
        public string Message { get; set; } = "";
        public string FormattedMessage { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class DebugLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Action<LogLevel, string, string> _logAction;

        public DebugLogger(string categoryName, Action<LogLevel, string, string> logAction)
        {
            _categoryName = categoryName;
            _logAction = logAction;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            _logAction.Invoke(logLevel, _categoryName, message);
        }
    }
}
