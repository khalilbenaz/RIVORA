FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KBA.AI.RAG.App.sln", "./"]
COPY ["src/KBA.AI.RAG.App.Api/KBA.AI.RAG.App.Api.csproj", "src/KBA.AI.RAG.App.Api/"]
COPY ["src/KBA.AI.RAG.App.Application/KBA.AI.RAG.App.Application.csproj", "src/KBA.AI.RAG.App.Application/"]
COPY ["src/KBA.AI.RAG.App.Domain/KBA.AI.RAG.App.Domain.csproj", "src/KBA.AI.RAG.App.Domain/"]
COPY ["src/KBA.AI.RAG.App.Infrastructure/KBA.AI.RAG.App.Infrastructure.csproj", "src/KBA.AI.RAG.App.Infrastructure/"]
COPY ["src/KBA.AI.RAG.App.RAG/KBA.AI.RAG.App.RAG.csproj", "src/KBA.AI.RAG.App.RAG/"]
COPY ["src/KBA.AI.RAG.App.AI/KBA.AI.RAG.App.AI.csproj", "src/KBA.AI.RAG.App.AI/"]
COPY ["src/KBA.AI.RAG.App.VectorStore/KBA.AI.RAG.App.VectorStore.csproj", "src/KBA.AI.RAG.App.VectorStore/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/KBA.AI.RAG.App.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KBA.AI.RAG.App.Api.dll"]
