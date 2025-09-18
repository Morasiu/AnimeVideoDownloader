using BlazorComponents.Services.Data.Models.Animes;
using Microsoft.EntityFrameworkCore;

namespace BlazorComponents.Services.Data;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<Anime> Anime { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}