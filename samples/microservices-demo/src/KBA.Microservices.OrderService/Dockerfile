FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KBA.Microservices.Demo.sln", "./"]
COPY ["src/KBA.Microservices.OrderService/KBA.Microservices.OrderService.csproj", "src/KBA.Microservices.OrderService/"]
COPY ["src/KBA.Microservices.Shared.Domain/KBA.Microservices.Shared.Domain.csproj", "src/KBA.Microservices.Shared.Domain/"]
COPY ["src/KBA.Microservices.Shared.Application/KBA.Microservices.Shared.Application.csproj", "src/KBA.Microservices.Shared.Application/"]
COPY ["src/KBA.Microservices.Shared.Infrastructure/KBA.Microservices.Shared.Infrastructure.csproj", "src/KBA.Microservices.Shared.Infrastructure/"]
COPY ["src/KBA.Microservices.Shared.Grpc/KBA.Microservices.Shared.Grpc.csproj", "src/KBA.Microservices.Shared.Grpc/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/KBA.Microservices.OrderService"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KBA.Microservices.OrderService.dll"]
