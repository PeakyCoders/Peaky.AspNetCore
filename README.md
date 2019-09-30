# Peaky.Extensions

## Minimal (default) Installation

1. Install the nuget on your ASP.NET Core project
2. On your `Startup` class inside your `ConfigureServices`method call the extension Configure method in the same way as other Middlewares : `services.ConfigureDebugLogging();`
3. Also in the `Startup` you need to add one last line more inside you `Configure`method : `app.UseDebugLogging();` .

