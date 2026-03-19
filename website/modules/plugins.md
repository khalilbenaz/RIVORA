# Plugin System

**Package** : `RVR.Framework.Plugins`

## Description

Systeme de plugins base sur NuGet pour RIVORA Framework. Permet la decouverte, l'installation, la verification et le chargement automatique de plugins, avec validation de compatibilite et verification de signature.

## Enregistrement

```csharp
builder.Services.AddRvrPluginSystem(builder.Configuration);
```

## Configuration

```json
{
  "Plugins": {
    "Enabled": true,
    "AutoDiscovery": true,
    "PluginDirectory": "./plugins",
    "NuGet": {
      "Sources": [
        "https://api.nuget.org/v3/index.json"
      ],
      "PackagePrefix": "RVR.Plugin.",
      "AllowPrerelease": false
    },
    "Signature": {
      "VerifySignatures": true,
      "AllowUnsigned": false,
      "TrustedSigners": ["Rivora", "KhalilBenazzouz"]
    },
    "Compatibility": {
      "StrictVersionCheck": true,
      "MinimumRivoraVersion": "3.0.0"
    }
  }
}
```

## Interface IRvrPlugin

```csharp
/// <summary>
/// Interface principale pour les plugins RIVORA.
/// Etend IPlugin avec des metadonnees specifiques au framework.
/// </summary>
public interface IRvrPlugin : IPlugin
{
    /// <summary>Version minimale de RIVORA requise.</summary>
    string MinimumRivoraVersion { get; }

    /// <summary>Liste des dependances (autres plugins requis).</summary>
    IReadOnlyList<string> Dependencies { get; }

    /// <summary>Metadonnees du plugin.</summary>
    PluginMetadata Metadata { get; }
}

public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }

    Task InitializeAsync(IServiceProvider services, CancellationToken ct = default);
    Task ShutdownAsync(CancellationToken ct = default);
}

public record PluginMetadata(
    string Author,
    string? ProjectUrl = null,
    string? LicenseUrl = null,
    IReadOnlyList<string>? Tags = null);
```

## Composants principaux

### NuGetPluginDiscovery

Recherche les packages NuGet correspondant au prefixe `RVR.Plugin.*` :

```csharp
public class NuGetPluginDiscovery
{
    /// <summary>Rechercher des plugins disponibles sur NuGet.</summary>
    Task<IReadOnlyList<PluginInfo>> SearchAsync(
        string? query = null, CancellationToken ct = default);

    /// <summary>Obtenir les details d'un plugin specifique.</summary>
    Task<PluginInfo?> GetInfoAsync(
        string packageId, CancellationToken ct = default);
}

public record PluginInfo(
    string PackageId,
    string LatestVersion,
    string Description,
    string Author,
    long DownloadCount,
    bool IsSigned);
```

### PluginInstaller

Installe et desinstalle les plugins depuis NuGet :

```csharp
public class PluginInstaller
{
    /// <summary>Installer un plugin depuis NuGet.</summary>
    Task<InstallResult> InstallAsync(
        string packageId, string? version = null, CancellationToken ct = default);

    /// <summary>Desinstaller un plugin.</summary>
    Task<bool> UninstallAsync(
        string packageId, CancellationToken ct = default);

    /// <summary>Lister les plugins installes.</summary>
    Task<IReadOnlyList<InstalledPlugin>> ListInstalledAsync(
        CancellationToken ct = default);

    /// <summary>Mettre a jour un plugin vers la derniere version.</summary>
    Task<InstallResult> UpdateAsync(
        string packageId, CancellationToken ct = default);
}
```

### PluginSignatureVerifier

Verifie la signature NuGet des packages :

```csharp
public class PluginSignatureVerifier
{
    /// <summary>Verifier la signature d'un package plugin.</summary>
    Task<SignatureResult> VerifyAsync(
        string packagePath, CancellationToken ct = default);
}

public record SignatureResult(
    bool IsValid,
    bool IsSigned,
    string? SignerName,
    string? Reason);
```

### PluginCompatibilityChecker

Valide la compatibilite avec la version courante de RIVORA :

```csharp
public class PluginCompatibilityChecker
{
    /// <summary>Verifier qu'un plugin est compatible avec la version actuelle.</summary>
    CompatibilityResult Check(IRvrPlugin plugin);
}

public record CompatibilityResult(
    bool IsCompatible,
    string? RequiredVersion,
    string? CurrentVersion,
    IReadOnlyList<string>? MissingDependencies);
```

## Utilisation

### Decouvrir et installer des plugins

```csharp
public class PluginManagerService
{
    private readonly NuGetPluginDiscovery _discovery;
    private readonly PluginInstaller _installer;
    private readonly PluginSignatureVerifier _verifier;

    public async Task<IReadOnlyList<PluginInfo>> RechercherPluginsAsync(CancellationToken ct)
    {
        return await _discovery.SearchAsync(query: null, ct);
    }

    public async Task InstallerPluginAsync(string packageId, CancellationToken ct)
    {
        // 1. Recuperer les informations
        var info = await _discovery.GetInfoAsync(packageId, ct);
        if (info is null)
            throw new InvalidOperationException($"Plugin {packageId} introuvable.");

        // 2. Installer depuis NuGet
        var result = await _installer.InstallAsync(packageId, ct: ct);
        if (!result.Success)
            throw new InvalidOperationException($"Echec d'installation : {result.Error}");

        // 3. Verifier la signature
        var signature = await _verifier.VerifyAsync(result.PackagePath, ct);
        if (!signature.IsValid)
        {
            await _installer.UninstallAsync(packageId, ct);
            throw new SecurityException($"Signature invalide pour {packageId}");
        }
    }
}
```

### Lister les plugins installes

```csharp
public async Task<IActionResult> GetInstalledPlugins(CancellationToken ct)
{
    var plugins = await _installer.ListInstalledAsync(ct);
    return Ok(plugins.Select(p => new
    {
        p.PackageId,
        p.Version,
        p.Description,
        p.InstalledAt
    }));
}
```

### Auto-decouverte par assembly scanning

Lorsque `AutoDiscovery` est active, RIVORA scanne automatiquement les assemblies dans le repertoire `PluginDirectory` et enregistre tous les types implementant `IRvrPlugin` :

```csharp
// Au demarrage, les plugins sont decouverts et initialises automatiquement
builder.Services.AddRvrPluginSystem(builder.Configuration);

// Equivalent manuel :
builder.Services.AddRvrPluginSystem(options =>
{
    options.AutoDiscovery = true;
    options.PluginDirectory = "./plugins";
    options.OnPluginLoaded = (plugin, sp) =>
    {
        Console.WriteLine($"Plugin charge : {plugin.Name} v{plugin.Version}");
    };
});
```

## Creer un plugin

Creez un projet de classe library ciblant .NET 9 :

```csharp
public class MonPlugin : IRvrPlugin
{
    public string Name => "MonPlugin";
    public string Version => "1.0.0";
    public string Description => "Un plugin personnalise pour RIVORA";
    public string MinimumRivoraVersion => "3.0.0";
    public IReadOnlyList<string> Dependencies => Array.Empty<string>();
    public PluginMetadata Metadata => new(
        Author: "Mon Equipe",
        ProjectUrl: "https://github.com/mon-equipe/mon-plugin",
        Tags: new[] { "custom", "example" });

    public async Task InitializeAsync(IServiceProvider services, CancellationToken ct)
    {
        var logger = services.GetRequiredService<ILogger<MonPlugin>>();
        logger.LogInformation("MonPlugin initialise avec succes");

        // Enregistrer vos services, middleware, etc.
    }

    public Task ShutdownAsync(CancellationToken ct)
    {
        // Nettoyage des ressources
        return Task.CompletedTask;
    }
}
```

Publiez le package avec le prefixe `RVR.Plugin.` sur NuGet pour qu'il soit decouvert automatiquement.

Voir l'exemple complet dans [samples/custom-plugin](https://github.com/khalilbenaz/RIVORA/tree/main/samples/custom-plugin).

## Fonctionnalites cles

- **Decouverte NuGet** : Recherche automatique des packages `RVR.Plugin.*`
- **Installation / Desinstallation** : Gestion complete du cycle de vie des plugins
- **Verification de signature** : Validation des signatures NuGet pour la securite
- **Compatibilite** : Verification de la version minimale de RIVORA et des dependances
- **Auto-decouverte** : Chargement automatique via assembly scanning au demarrage
- **Metadonnees enrichies** : Auteur, URL, licence, tags pour chaque plugin
- **Extensible** : Interface `IRvrPlugin` simple a implementer
