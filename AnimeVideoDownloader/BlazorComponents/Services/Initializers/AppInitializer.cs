namespace BlazorComponents.Services.Initializers;

public sealed class AppInitializer
{
    private readonly IEnumerable<IInitializer> _initializers;

    public AppInitializer(IEnumerable<IInitializer> initializers)
    {
        _initializers = initializers;
    }

    public async Task InitAsync()
    {
        var tasks = new List<Task>();
        foreach (var initializer in _initializers)
        {
            var task = initializer.InitAsync();
            tasks.Add(task);
        }
        
        await Task.WhenAll(tasks);
    }
}