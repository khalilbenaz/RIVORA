FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KBA.Microservices.Demo.sln", "./"]
COPY ["src/KBA.Microservices.ApiGateway/KBA.Microservices.ApiGateway.csproj", "src/KBA.Microservices.ApiGateway/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/KBA.Microservices.ApiGateway"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KBA.Microservices.ApiGateway.dll"]
