FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KBA.Microservices.Demo.sln", "./"]
COPY ["src/KBA.Microservices.NotificationService/KBA.Microservices.NotificationService.csproj", "src/KBA.Microservices.NotificationService/"]
COPY ["src/KBA.Microservices.Shared.Application/KBA.Microservices.Shared.Application.csproj", "src/KBA.Microservices.Shared.Application/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/KBA.Microservices.NotificationService"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KBA.Microservices.NotificationService.dll"]
