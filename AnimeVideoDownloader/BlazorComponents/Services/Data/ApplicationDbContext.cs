using Microsoft.EntityFrameworkCore;

namespace BlazorComponents.Services.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
}