# Project Guidelines

This document summarizes the conventions and Blazor-specific practices used in this repository, based on the current codebase.

## Coding Conventions

- General C#
  - Language/Target: .NET 9 where applicable (projects include net9.0, net9.0-windows, netstandard2.0 as needed).
  - Nullable Reference Types: Enabled and used. Prefer explicit nullability annotations and the null-forgiving operator (`!`) only when safe (e.g., EditorRequired parameters in components).
  - Async: Asynchronous methods use the `Async` suffix (e.g., `DownloadAsync`). Prefer `Task`/`Task<T>` and pass `CancellationToken` with a default (`ct = default`).
  - Namespaces: Use file-scoped namespaces (e.g., `namespace BlazorComponents.Services.Logging;`).
  - Classes:
    - Services: `public sealed` where appropriate.
    - Use access modifiers explicitly (e.g., `private readonly` fields for DI).
  - Fields and Properties:
    - Private fields: underscore-prefixed camelCase (e.g., `_logger`, `_youtubeDL`).
    - Use C# `required` members for DTO-like types where initialization is mandatory (e.g., progress reporting model).
  - Logging:
    - Use `ILogger<T>` with structured logging message templates and named parameters (e.g., `LogInformation("Starting downloading from {Url}", url)`).
    - Errors logged via `LogError` with meaningful context and aggregated messages if needed.
  - Collections and LINQ:
    - Favor LINQ for collection inspection (e.g., `Any`, `All`).
  - Option/Config Objects:
    - Use strongly-typed options where provided by libraries (e.g., `OptionSet.Default` from YoutubeDLSharp) and adjust specific flags.

- Dependency Injection (DI)
  - Registration via extension methods on `IServiceCollection` in `...ServiceCollectionExtensions` classes.
  - Lifetimes:
    - `AddSingleton` for application-wide services and initializers.
  - Logger providers can be registered by adding a custom provider through `ILoggingBuilder.AddProvider(...)`.

- Folder and Naming Structure
  - Blazor project layout under `BlazorComponents`:
    - Components in `Components` (with subfolders by domain, e.g., `Components/Anime`).
    - Pages in `Pages` with `@page` directives.
    - Services under `Services` (organized by feature: `Logging`, `Initializers`, `YoutubeDLService`, etc.).
    - Static assets under `wwwroot`.
  - Class and file names use PascalCase and match the contained type/component.
  - Extension classes named `*ServiceCollectionExtensions` expose `Add*` methods.

- Error Handling
  - Prefer explicit status reporting and logging. For background/process flows, propagate errors via status objects (e.g., progress object includes `Error`).

## Blazor-Specific Practices

- Components
  - File naming: `ComponentName.razor` with optional CSS isolation in `ComponentName.razor.css`.
  - Do not use inline CSS styles
  - Parameters:
    - Mark parameters with `[Parameter]` and use `[EditorRequired]` for required inputs.
    - Use `EventCallback<T>` for parent-child communication of events/actions.
  - Dependency Injection inside components via:
    - `@inject` directive at the top or `[Inject]` properties in `@code` block.
  - Lifecycle:
    - Override `OnInitialized()` for initial data/state setup; call `base.OnInitialized()` when overriding.
    - Use `InvokeAsync(StateHasChanged)` when updating state from non-UI threads (e.g., event callbacks, timers, Task continuations).
  - Disposing:
    - Implement `IDisposable` for components that subscribe to external events and unsubscribe in `Dispose()`; add `@implements IDisposable` in the `.razor` file.
  - Rendering and Events:
    - Use conditional rendering with `if/else` and `@switch` blocks for status visualization.
    - Hook UI events with `@onclick` and async handlers that return `Task`.
    - Use CSS class composition to reflect state (e.g., status classes derived from service state methods).

- Styling
  - Prefer CSS isolation per component: styles are placed in `*.razor.css` files (e.g., `Debug.razor.css`).
  - Use semantic class names and keep icon usage consistent (e.g., Font Awesome classes for status icons).

- Services & Background Work
  - Inject services into components for actions (e.g., `DownloaderService` for downloads).
  - Report progress to the UI via `IProgress<T>` models; log progress with structured messages.
  - For long-running operations, pass a `CancellationToken` sourced in the component (e.g., `CancellationTokenSource cts`).

- Initializers & App Status
  - Centralize app startup checks in initializer services (e.g., `AppInitializer` managing a collection of `IInitializer` implementations like `PlaywrightInitializer`, `YoutubeDLInitializer`).
  - Expose results via read-only collections and raise events for status changes; components subscribe and update UI accordingly.

## Practical Examples from Codebase

- Required component parameter with editor enforcement:
  - `[Parameter, EditorRequired] public AnimeModel Anime { get; set; } = null!;`
- Structured logging:
  - `_logger.LogInformation("Starting downloading from {Url}", url);`
  - `Logger.LogInformation("Download progress - Status: {Status}, Progress: {Progress:P}, Speed: {Speed}, ETA: {Eta}, TotalBytes: {TotalBytes}, Error: {Error}", ...)`
- DI registration pattern:
  - `public static IServiceCollection AddAppInitializers(this IServiceCollection services) { services.AddSingleton<AppInitializer>(); services.AddSingleton<IInitializer, PlaywrightInitializer>(); services.AddSingleton<IInitializer, YoutubeDLInitializer>(); return services; }`

## Notes and Recommendations

- Avoid overusing `BuildServiceProvider()` within registration lambdas to resolve services; prefer using `services.AddSingleton<ILoggerProvider>(...)` or defer provider creation until runtime if possible. Current logging extension builds a provider to obtain `MemoryLogService`; consider refactoring to standard patterns in future iterations.
- Continue enforcing `EditorRequired` for must-have parameters and use nullability annotations consistently.
- Keep component responsibilities focused and delegate heavy work to injected services.
