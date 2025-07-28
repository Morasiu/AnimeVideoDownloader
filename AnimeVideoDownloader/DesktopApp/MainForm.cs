using BlazorComponents;
using BlazorComponents.Extensions;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApp;
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        var services = new ServiceCollection();
        services.AddBlazorComponentsServices();
        services.AddWindowsFormsBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();
        
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<MainForm>>();
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