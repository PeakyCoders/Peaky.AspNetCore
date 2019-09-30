# Peaky.AspNetCore

## Peaky.AspNetCore.Debugging 

#### Recommended usage

1. Install the NuGet in your ASP.NET Core project
2. In your `Startup` class, inside `ConfigureServices()` method, call `services.ConfigureDebugLogging();` (optionally, inside an `#if DEBUG` conditional block)
3. In your `Startup` class, inside `Configure()` method, call `app.UseDebugLogging();`

```csharp
public void ConfigureServices(IServiceCollection services)
{
	#if DEBUG
	services.ConfigureDebugLogging();
	#endif
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
	#if DEBUG
    app.UseDebugLogging();
	#endif
}
```