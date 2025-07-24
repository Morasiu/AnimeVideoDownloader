using BlazorComponents;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApp;
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
        BlazorWebView blazor = new BlazorWebView()
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html",
            Services = services.BuildServiceProvider(),
        };
        blazor.RootComponents.Add<App>("#app");
        
        Controls.Add(blazor);
    }
}