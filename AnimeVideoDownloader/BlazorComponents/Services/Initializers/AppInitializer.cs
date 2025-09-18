using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.Initializers;

public sealed class AppInitializer
{
    public readonly ReadOnlyObservableCollection<InitResult> Results = new(_results);
    public event Action? StatusChanged;
    
    private static readonly ObservableCollection<InitResult> _results = [];
    private readonly IEnumerable<IInitializer> _initializers;
    private readonly ILogger<AppInitializer> _logger;

    public AppInitializer(IEnumerable<IInitializer> initializers, ILogger<AppInitializer> logger)
    {
        _initializers = initializers;
        _logger = logger;
    }

    public async Task InitAsync()
    {
        var tasks = new List<Task>();
        foreach (var initializer in _initializers)
        {
            var initResult = new InitResult { Name = initializer.GetType().Name, Status = InitStatus.Pending };
            _results.Add(initResult);
            StatusChanged?.Invoke();
            var task = Task.Run(initializer.InitAsync).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    initResult.Status = InitStatus.Success;
                }
                else
                {
                    initResult.Exception = task.Exception;
                    initResult.Status = InitStatus.Error;
                    _logger.LogError(task.Exception, "Failed to initialize {Name}", initializer.GetType().Name);
                }
                StatusChanged?.Invoke();
            });
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);
    }
}