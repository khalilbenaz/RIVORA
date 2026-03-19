# RVR.Plugin.Sample

A minimal sample plugin for the RIVORA Framework demonstrating the `IRvrPlugin` interface.

## Overview

This sample shows how to create a RIVORA plugin that:

- Implements `IRvrPlugin` with version compatibility, dependency declarations, and metadata
- Registers custom services in `Initialize(IServiceCollection)`
- Adds endpoints in `Configure(IApplicationBuilder)`

## Creating Your Own Plugin

### 1. Create a class library

```bash
dotnet new classlib -n RVR.Plugin.MyPlugin -f net9.0
```

### 2. Reference the RIVORA Framework

Add project or package references to `RVR.Framework.Core` and `RVR.Framework.Plugins`.

### 3. Implement `IRvrPlugin`

```csharp
public sealed class MyPlugin : IRvrPlugin
{
    public string Name => "RVR.Plugin.MyPlugin";
    public string Version => "1.0.0";
    public string MinimumRivoraVersion => "3.1.0";
    public IEnumerable<string> Dependencies => [];
    public PluginMetadata Metadata => new(
        Author: "Your Name",
        Description: "What your plugin does.");

    public void Initialize(IServiceCollection services)
    {
        // Register your services here
    }

    public void Configure(IApplicationBuilder app)
    {
        // Add middleware or endpoints here
    }
}
```

### 4. Build and deploy

Copy the output DLL to the host application's `plugins/` directory, or publish as a NuGet package following the `RVR.Plugin.*` naming convention for auto-discovery.

### 5. Publish to NuGet (optional)

```bash
dotnet pack -c Release
dotnet nuget push bin/Release/RVR.Plugin.MyPlugin.1.0.0.nupkg -s https://api.nuget.org/v3/index.json -k YOUR_API_KEY
```

Once published, the host application can discover and install your plugin using `NuGetPluginDiscovery` and `PluginInstaller`.

## Registration in the Host Application

```csharp
// Program.cs or Startup.cs
builder.Services.AddRvrPluginSystem(builder.Configuration);
builder.Services.AddRvrPluginAutoDiscovery();

var app = builder.Build();
app.UseRvrPlugins();
```
