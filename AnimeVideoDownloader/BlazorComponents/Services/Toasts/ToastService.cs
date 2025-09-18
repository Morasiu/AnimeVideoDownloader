using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.Toasts;

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}

public sealed class ToastMessage
{
    public required string Id { get; init; }
    public required string Message { get; init; }
    public string? Title { get; init; }
    public ToastType Type { get; init; } = ToastType.Info;
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(4);
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;

    internal CancellationTokenSource? Cts { get; set; }
}

public sealed class ToastService
{
    public event Action? Changed;

    // ReSharper disable once InconsistentlySynchronizedField
    public ReadOnlyObservableCollection<ToastMessage> Messages => new(_messages);

    private static readonly ObservableCollection<ToastMessage> _messages = [];
    private readonly ILogger<ToastService> _logger;
    private readonly object _gate = new();

    public ToastService(ILogger<ToastService> logger)
    {
        _logger = logger;
    }

    public string Show(string message, string? title = null, ToastType type = ToastType.Info, TimeSpan? duration = null, CancellationToken ct = default)
    {
        var id = Guid.NewGuid().ToString("N");
        var toast = new ToastMessage
        {
            Id = id,
            Message = message,
            Title = title,
            Type = type,
            Duration = duration ?? TimeSpan.FromSeconds(4),
            CreatedAt = DateTimeOffset.Now
        };

        lock (_gate)
        {
            _messages.Add(toast);
        }
        Changed?.Invoke();

        // schedule auto-dismiss
        toast.Cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(toast.Duration, toast.Cts.Token);
                Dismiss(id);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while auto-dismissing toast {ToastId}", id);
            }
        });

        return id;
    }

    public string ShowInfo(string message, string? title = null, TimeSpan? duration = null, CancellationToken ct = default) =>
        Show(message, title, ToastType.Info, duration, ct);

    public string ShowSuccess(string message, string? title = null, TimeSpan? duration = null, CancellationToken ct = default) =>
        Show(message, title, ToastType.Success, duration, ct);

    public string ShowWarning(string message, string? title = null, TimeSpan? duration = null, CancellationToken ct = default) =>
        Show(message, title, ToastType.Warning, duration, ct);

    public string ShowError(string message, string? title = null, TimeSpan? duration = null, CancellationToken ct = default) =>
        Show(message, title, ToastType.Error, duration, ct);

    public void Dismiss(string id)
    {
        ToastMessage? removed = null;
        lock (_gate)
        {
            var found = _messages.FirstOrDefault(t => t.Id == id);
            if (found != null)
            {
                _messages.Remove(found);
                removed = found;
            }
        }
        if (removed != null)
        {
            try { removed.Cts?.Cancel(); } catch { /* ignore */ }
            Changed?.Invoke();
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            foreach (var msg in _messages.ToList())
            {
                try { msg.Cts?.Cancel(); } catch { /* ignore */ }
            }
            _messages.Clear();
        }
        Changed?.Invoke();
    }
}
