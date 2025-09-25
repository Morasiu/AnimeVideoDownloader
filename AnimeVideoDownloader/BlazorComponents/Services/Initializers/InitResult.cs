namespace BlazorComponents.Services.Initializers;

public class InitResult
{
    public required string Name { get; set; }
    public InitStatus Status { get; set; }
    public Exception? Exception { get; set; }
}

public enum InitStatus
{
    Success,
    Error,
    Pending,
}