FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RVR.Microservices.Demo.sln", "./"]
COPY ["src/RVR.Microservices.NotificationService/RVR.Microservices.NotificationService.csproj", "src/RVR.Microservices.NotificationService/"]
COPY ["src/RVR.Microservices.Shared.Application/RVR.Microservices.Shared.Application.csproj", "src/RVR.Microservices.Shared.Application/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/RVR.Microservices.NotificationService"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RVR.Microservices.NotificationService.dll"]
