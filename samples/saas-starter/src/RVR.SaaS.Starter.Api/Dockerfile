# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["RVR.SaaS.Starter.sln", "./"]
COPY ["src/RVR.SaaS.Starter.Api/RVR.SaaS.Starter.Api.csproj", "src/RVR.SaaS.Starter.Api/"]
COPY ["src/RVR.SaaS.Starter.Application/RVR.SaaS.Starter.Application.csproj", "src/RVR.SaaS.Starter.Application/"]
COPY ["src/RVR.SaaS.Starter.Domain/RVR.SaaS.Starter.Domain.csproj", "src/RVR.SaaS.Starter.Domain/"]
COPY ["src/RVR.SaaS.Starter.Infrastructure/RVR.SaaS.Starter.Infrastructure.csproj", "src/RVR.SaaS.Starter.Infrastructure/"]
COPY ["src/RVR.SaaS.Starter.Identity/RVR.SaaS.Starter.Identity.csproj", "src/RVR.SaaS.Starter.Identity/"]

# Restore
RUN dotnet restore "src/RVR.SaaS.Starter.Api/RVR.SaaS.Starter.Api.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/src/RVR.SaaS.Starter.Api"
RUN dotnet publish "RVR.SaaS.Starter.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80

# Install dependencies for Hangfire dashboard
RUN apt-get update && apt-get install -y fontconfig

COPY --from=build /app/publish .
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1
USER app
ENTRYPOINT ["dotnet", "RVR.SaaS.Starter.Api.dll"]
