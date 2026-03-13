using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace KBA.CLI.Commands;

public static class DockerCommand
{
    public static async Task ExecuteAsync(string database)
    {
        AnsiConsole.MarkupLine("[bold blue]🐳 Generating Docker environment...[/]");

        var currentDir = Directory.GetCurrentDirectory();
        
        // Find the API project dynamically
        string apiProjectName = "KBA.Framework.Api"; // Default
        var srcDir = Path.Combine(currentDir, "src");
        if (Directory.Exists(srcDir))
        {
            var apiDir = Directory.GetDirectories(srcDir, "*.Api").FirstOrDefault();
            if (apiDir != null)
            {
                apiProjectName = Path.GetFileName(apiDir);
            }
        }

        // Generate Dockerfile
        var dockerfileContent = $@"FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY [""src/{apiProjectName}/{apiProjectName}.csproj"", ""src/{apiProjectName}/""]
RUN dotnet restore ""src/{apiProjectName}/{apiProjectName}.csproj""
COPY . .
WORKDIR ""/src/src/{apiProjectName}""
RUN dotnet build ""{apiProjectName}.csproj"" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ""{apiProjectName}.csproj"" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [""dotnet"", ""{apiProjectName}.dll""]
";
        await File.WriteAllTextAsync("Dockerfile", dockerfileContent);
        AnsiConsole.MarkupLine("[green]✓[/] Dockerfile created");

        // Generate docker-compose.yml
        var dbImage = database.ToLower() == "postgresql" ? "postgres:15-alpine" : "mcr.microsoft.com/mssql/server:2022-latest";
        var dbPort = database.ToLower() == "postgresql" ? "5432:5432" : "1433:1433";
        var dbEnv = database.ToLower() == "postgresql" 
            ? "      - POSTGRES_PASSWORD=SuperSecret123!\n      - POSTGRES_USER=postgres\n      - POSTGRES_DB=kbadb" 
            : "      - ACCEPT_EULA=Y\n      - SA_PASSWORD=SuperSecret123!";

        var composeContent = $@"version: '3.8'

services:
  api:
    image: kba-api
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - ""8080:8080""
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=db;Database=kbadb;Username=postgres;Password=SuperSecret123!
    depends_on:
      - db
      - redis

  db:
    image: {dbImage}
    ports:
      - ""{dbPort}""
    environment:
{dbEnv}
    volumes:
      - kba-data:/var/lib/postgresql/data

  redis:
    image: redis:alpine
    ports:
      - ""6379:6379""

volumes:
  kba-data:
";
        await File.WriteAllTextAsync("docker-compose.yml", composeContent);
        AnsiConsole.MarkupLine("[green]✓[/] docker-compose.yml created");

        AnsiConsole.MarkupLine("\n[yellow]To start your environment:[/]");
        AnsiConsole.MarkupLine("  docker compose up -d");
    }
}
