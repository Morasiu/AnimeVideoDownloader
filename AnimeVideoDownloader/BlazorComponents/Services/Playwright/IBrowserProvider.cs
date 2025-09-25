using Microsoft.Playwright;

namespace BlazorComponents.Services.Playwright;

public interface IBrowserProvider
{
    IBrowser GetBrowser();
}