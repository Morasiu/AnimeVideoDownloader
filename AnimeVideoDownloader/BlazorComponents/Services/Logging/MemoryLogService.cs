using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.Logging;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public required string Category { get; set; }
    public required string Message { get; set; }
    public Exception? Exception { get; set; }
    public EventId EventId { get; set; }
}

public class MemoryLogService
{
    private static readonly ObservableCollection<LogEntry> _logs = new();
    private readonly int _maxLogs = 1000;
    public ObservableCollection<LogEntry> Logs => _logs;


    public void AddLog(LogEntry logEntry)
    {
        _logs.Add(logEntry);
        if (_logs.Count > _maxLogs)
        {
            _logs.RemoveAt(0);
        }
    }

    public void ClearLogs()
    {
        _logs.Clear();
    }
}

public class MemoryLogger : ILogger
{
    private readonly string _categoryName;
    private readonly MemoryLogService _logService;

    public MemoryLogger(string categoryName, MemoryLogService logService)
    {
        _categoryName = categoryName;
        _logService = logService;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = logLevel,
            Category = _categoryName,
            Message = formatter(state, exception),
            Exception = exception,
            EventId = eventId
        };
        _logService.AddLog(logEntry);
    }
}

public class MemoryLoggerProvider : ILoggerProvider
{
    private readonly MemoryLogService _logService;

    public MemoryLoggerProvider(MemoryLogService logService)
    {
        _logService = logService;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new MemoryLogger(categoryName, _logService);
    }

    public void Dispose() {}
}