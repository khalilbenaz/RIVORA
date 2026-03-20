FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RVR.Microservices.Demo.sln", "./"]
COPY ["src/RVR.Microservices.ProductService/RVR.Microservices.ProductService.csproj", "src/RVR.Microservices.ProductService/"]
COPY ["src/RVR.Microservices.Shared.Domain/RVR.Microservices.Shared.Domain.csproj", "src/RVR.Microservices.Shared.Domain/"]
COPY ["src/RVR.Microservices.Shared.Application/RVR.Microservices.Shared.Application.csproj", "src/RVR.Microservices.Shared.Application/"]
COPY ["src/RVR.Microservices.Shared.Infrastructure/RVR.Microservices.Shared.Infrastructure.csproj", "src/RVR.Microservices.Shared.Infrastructure/"]
COPY ["src/RVR.Microservices.Shared.Grpc/RVR.Microservices.Shared.Grpc.csproj", "src/RVR.Microservices.Shared.Grpc/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/RVR.Microservices.ProductService"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1
USER app
ENTRYPOINT ["dotnet", "RVR.Microservices.ProductService.dll"]
