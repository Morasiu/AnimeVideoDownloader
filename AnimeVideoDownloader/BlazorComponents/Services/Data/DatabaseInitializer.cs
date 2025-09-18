using BlazorComponents.Services.Initializers;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.Data;

public sealed class DatabaseInitializer : IInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(ApplicationDbContext context, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task InitAsync()
    {
        _logger.LogInformation("Creating database");
        await _context.Database.EnsureCreatedAsync();
        _logger.LogInformation("Creating database finished");
    }
}