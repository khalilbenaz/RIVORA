FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RVR.Microservices.Demo.sln", "./"]
COPY ["src/RVR.Microservices.ApiGateway/RVR.Microservices.ApiGateway.csproj", "src/RVR.Microservices.ApiGateway/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/RVR.Microservices.ApiGateway"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RVR.Microservices.ApiGateway.dll"]
