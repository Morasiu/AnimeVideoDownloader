using BlazorComponents.Services.AppData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlazorComponents.Services.Data;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite($"Data Source={AppDataPath.AnimeDownloaderPath}/anime.db")
            .EnableSensitiveDataLogging()
            .UseLazyLoadingProxies();

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}