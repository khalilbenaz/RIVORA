# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["KBA.SaaS.Starter.sln", "./"]
COPY ["src/KBA.SaaS.Starter.Api/KBA.SaaS.Starter.Api.csproj", "src/KBA.SaaS.Starter.Api/"]
COPY ["src/KBA.SaaS.Starter.Application/KBA.SaaS.Starter.Application.csproj", "src/KBA.SaaS.Starter.Application/"]
COPY ["src/KBA.SaaS.Starter.Domain/KBA.SaaS.Starter.Domain.csproj", "src/KBA.SaaS.Starter.Domain/"]
COPY ["src/KBA.SaaS.Starter.Infrastructure/KBA.SaaS.Starter.Infrastructure.csproj", "src/KBA.SaaS.Starter.Infrastructure/"]
COPY ["src/KBA.SaaS.Starter.Identity/KBA.SaaS.Starter.Identity.csproj", "src/KBA.SaaS.Starter.Identity/"]

# Restore
RUN dotnet restore "src/KBA.SaaS.Starter.Api/KBA.SaaS.Starter.Api.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/src/KBA.SaaS.Starter.Api"
RUN dotnet publish "KBA.SaaS.Starter.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80

# Install dependencies for Hangfire dashboard
RUN apt-get update && apt-get install -y fontconfig

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "KBA.SaaS.Starter.Api.dll"]
