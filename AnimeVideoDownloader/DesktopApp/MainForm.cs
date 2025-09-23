using BlazorComponents;
using BlazorComponents.Extensions;
using BlazorComponents.Services.Data;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace DesktopApp;
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        Icon = new Icon(Application.StartupPath + "/icon.ico");
        var services = new ServiceCollection();
        services.AddBlazorComponentsServices();
        services.AddLogging(l => l
            .AddConsole()
            .AddFile("anime_downloader.log", o =>
            {
                o.Append = true;
                o.FileSizeLimitBytes = 1024 * 1024 * 10;
            }));
        services.AddWindowsFormsBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();
        
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MainForm>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Starting Database Migration");
        context.Database.Migrate();
        logger.LogInformation("Starting Database Migration finished");
        logger.LogInformation("Starting Blazor Host");
        BlazorWebView blazor = new BlazorWebView
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html",
            Services = provider,
        };
        blazor.RootComponents.Add<App>("#app");
        Controls.Add(blazor);
    }
}