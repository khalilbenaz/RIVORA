FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RVR.AI.RAG.App.sln", "./"]
COPY ["src/RVR.AI.RAG.App.Api/RVR.AI.RAG.App.Api.csproj", "src/RVR.AI.RAG.App.Api/"]
COPY ["src/RVR.AI.RAG.App.Application/RVR.AI.RAG.App.Application.csproj", "src/RVR.AI.RAG.App.Application/"]
COPY ["src/RVR.AI.RAG.App.Domain/RVR.AI.RAG.App.Domain.csproj", "src/RVR.AI.RAG.App.Domain/"]
COPY ["src/RVR.AI.RAG.App.Infrastructure/RVR.AI.RAG.App.Infrastructure.csproj", "src/RVR.AI.RAG.App.Infrastructure/"]
COPY ["src/RVR.AI.RAG.App.RAG/RVR.AI.RAG.App.RAG.csproj", "src/RVR.AI.RAG.App.RAG/"]
COPY ["src/RVR.AI.RAG.App.AI/RVR.AI.RAG.App.AI.csproj", "src/RVR.AI.RAG.App.AI/"]
COPY ["src/RVR.AI.RAG.App.VectorStore/RVR.AI.RAG.App.VectorStore.csproj", "src/RVR.AI.RAG.App.VectorStore/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/RVR.AI.RAG.App.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RVR.AI.RAG.App.Api.dll"]
