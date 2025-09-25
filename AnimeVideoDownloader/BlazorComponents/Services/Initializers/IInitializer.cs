namespace BlazorComponents.Services.Initializers;

public interface IInitializer
{
    bool IsInitialized { get; protected set; }
    Task InitAsync();
}

