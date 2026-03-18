using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace RVR.Framework.Admin.Endpoints;

/// <summary>
/// API endpoints for RVR Studio — solution generation and download.
/// </summary>
public static class StudioEndpoints
{
    /// <summary>
    /// Maps the Studio API endpoints.
    /// </summary>
    public static void MapStudioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/studio").WithTags("Studio");

        group.MapPost("/generate", GenerateSolution)
            .WithName("GenerateSolution")
            .WithDescription("Generate a RIVORA solution and return it as a ZIP archive");

        group.MapGet("/download/{name}", DownloadSolution)
            .WithName("DownloadSolution")
            .WithDescription("Download a previously generated solution as a ZIP archive");
    }

    private static async Task<IResult> GenerateSolution(SolutionRequest request)
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "rvr-studio", request.Name);

            // Clean up any previous generation
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            Directory.CreateDirectory(tempDir);

            // Generate solution structure
            GenerateSolutionFiles(tempDir, request);

            // Create ZIP in memory
            var zipPath = Path.Combine(Path.GetTempPath(), "rvr-studio", $"{request.Name}.zip");
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            ZipFile.CreateFromDirectory(tempDir, zipPath);

            var zipBytes = await File.ReadAllBytesAsync(zipPath);

            // Cleanup temp files
            Directory.Delete(tempDir, true);
            File.Delete(zipPath);

            return Results.File(zipBytes, "application/zip", $"{request.Name}.zip");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate solution: {ex.Message}");
        }
    }

    private static async Task<IResult> DownloadSolution(string name)
    {
        var zipPath = Path.Combine(Path.GetTempPath(), "rvr-studio", $"{name}.zip");
        if (!File.Exists(zipPath))
        {
            return Results.NotFound($"Solution '{name}' not found. Generate it first via POST /api/studio/generate.");
        }

        var zipBytes = await File.ReadAllBytesAsync(zipPath);
        return Results.File(zipBytes, "application/zip", $"{name}.zip");
    }

    private static void GenerateSolutionFiles(string rootDir, SolutionRequest request)
    {
        var name = request.Name;
        var ns = request.Namespace ?? name;

        // Create directory structure
        Directory.CreateDirectory(Path.Combine(rootDir, $"src/{name}.Domain/Entities"));
        Directory.CreateDirectory(Path.Combine(rootDir, $"src/{name}.Application/Features"));
        Directory.CreateDirectory(Path.Combine(rootDir, $"src/{name}.Infrastructure"));
        Directory.CreateDirectory(Path.Combine(rootDir, $"src/{name}.Api/Endpoints"));
        Directory.CreateDirectory(Path.Combine(rootDir, $"tests/{name}.Tests"));

        // Solution file
        var slnContent = $@"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{name}.Domain"", ""src\{name}.Domain\{name}.Domain.csproj"", ""{{{Guid.NewGuid().ToString("D").ToUpper()}}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{name}.Application"", ""src\{name}.Application\{name}.Application.csproj"", ""{{{Guid.NewGuid().ToString("D").ToUpper()}}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{name}.Infrastructure"", ""src\{name}.Infrastructure\{name}.Infrastructure.csproj"", ""{{{Guid.NewGuid().ToString("D").ToUpper()}}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{name}.Api"", ""src\{name}.Api\{name}.Api.csproj"", ""{{{Guid.NewGuid().ToString("D").ToUpper()}}}""
EndProject
Global
EndGlobal";
        File.WriteAllText(Path.Combine(rootDir, $"{name}.sln"), slnContent);

        // Directory.Build.props
        File.WriteAllText(Path.Combine(rootDir, "Directory.Build.props"), $@"<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13.0</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <Version>1.0.0</Version>
  </PropertyGroup>
</Project>");

        // Domain project
        File.WriteAllText(Path.Combine(rootDir, $"src/{name}.Domain/{name}.Domain.csproj"), $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <RootNamespace>{ns}.Domain</RootNamespace>
  </PropertyGroup>
</Project>");

        File.WriteAllText(Path.Combine(rootDir, $"src/{name}.Domain/Entities/SampleEntity.cs"), $@"namespace {ns}.Domain.Entities;

public class SampleEntity
{{
    public Guid Id {{ get; private set; }}
    public string Name {{ get; private set; }} = string.Empty;
    public DateTime CreatedAt {{ get; private set; }}

    public static SampleEntity Create(string name) => new()
    {{
        Id = Guid.NewGuid(),
        Name = name,
        CreatedAt = DateTime.UtcNow
    }};
}}");

        // Application project
        File.WriteAllText(Path.Combine(rootDir, $"src/{name}.Application/{name}.Application.csproj"), $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <RootNamespace>{ns}.Application</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=""..\{name}.Domain\{name}.Domain.csproj"" />
  </ItemGroup>
</Project>");

        // Infrastructure project
        var dbPackage = (request.Database ?? "postgresql") switch
        {
            "sqlserver" => @"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""9.0.3"" />",
            "mysql" => @"<PackageReference Include=""Pomelo.EntityFrameworkCore.MySql"" Version=""9.0.0"" />",
            "sqlite" => @"<PackageReference Include=""Microsoft.EntityFrameworkCore.Sqlite"" Version=""9.0.3"" />",
            _ => @"<PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL"" Version=""9.0.4"" />"
        };

        File.WriteAllText(Path.Combine(rootDir, $"src/{name}.Infrastructure/{name}.Infrastructure.csproj"), $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <RootNamespace>{ns}.Infrastructure</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""9.0.3"" />
    {dbPackage}
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\{name}.Domain\{name}.Domain.csproj"" />
    <ProjectReference Include=""..\{name}.Application\{name}.Application.csproj"" />
  </ItemGroup>
</Project>");

        // API project
        File.WriteAllText(Path.Combine(rootDir, $"src/{name}.Api/{name}.Api.csproj"), $@"<Project Sdk=""Microsoft.NET.Sdk.Web"">
  <PropertyGroup>
    <RootNamespace>{ns}.Api</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""7.2.0"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\{name}.Application\{name}.Application.csproj"" />
    <ProjectReference Include=""..\{name}.Infrastructure\{name}.Infrastructure.csproj"" />
  </ItemGroup>
</Project>");

        // Program.cs
        File.WriteAllText(Path.Combine(rootDir, $"src/{name}.Api/Program.cs"), $@"var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{{
    app.UseSwagger();
    app.UseSwaggerUI();
}}

app.MapGet(""/api/hello"", () => Results.Ok(new {{ message = ""Hello from {name}!"" }}));
app.MapHealthChecks(""/health"");

app.Run();
");

        // appsettings.json
        var connStr = (request.Database ?? "postgresql") switch
        {
            "sqlserver" => $"Server=localhost;Database={name.ToLower()}db;Trusted_Connection=True;TrustServerCertificate=True",
            "sqlite" => $"Data Source={name.ToLower()}.db",
            _ => $"Host=localhost;Database={name.ToLower()}db;Username=postgres;Password=postgres"
        };

        File.WriteAllText(Path.Combine(rootDir, $"src/{name}.Api/appsettings.json"), $@"{{
  ""Logging"": {{ ""LogLevel"": {{ ""Default"": ""Information"" }} }},
  ""ConnectionStrings"": {{ ""DefaultConnection"": ""{connStr}"" }}
}}");

        // Test project
        File.WriteAllText(Path.Combine(rootDir, $"tests/{name}.Tests/{name}.Tests.csproj"), $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.12.0"" />
    <PackageReference Include=""xunit"" Version=""2.9.3"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""3.0.2"" PrivateAssets=""all"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\..\src\{name}.Domain\{name}.Domain.csproj"" />
  </ItemGroup>
</Project>");

        File.WriteAllText(Path.Combine(rootDir, $"tests/{name}.Tests/SampleTests.cs"), $@"namespace {ns}.Tests;

public class SampleTests
{{
    [Fact]
    public void Placeholder_ShouldPass() => Assert.True(true);
}}");

        // .gitignore
        File.WriteAllText(Path.Combine(rootDir, ".gitignore"), @"bin/
obj/
.vs/
*.user
");
    }
}

/// <summary>
/// Request model for solution generation.
/// </summary>
public class SolutionRequest
{
    public string Name { get; set; } = "MyApp";
    public string? Namespace { get; set; }
    public string? AppType { get; set; }
    public string? Database { get; set; }
    public List<string>? Modules { get; set; }
    public List<string>? Security { get; set; }
    public string? TenancyMode { get; set; }
    public string? AiMode { get; set; }
    public List<string>? DevOps { get; set; }
}
