FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KBA.AI.RAG.App.sln", "./"]
COPY ["src/KBA.AI.RAG.App.Blazor/KBA.AI.RAG.App.Blazor.csproj", "src/KBA.AI.RAG.App.Blazor/"]
COPY ["src/KBA.AI.RAG.App.Application/KBA.AI.RAG.App.Application.csproj", "src/KBA.AI.RAG.App.Application/"]
COPY ["src/KBA.AI.RAG.App.Domain/KBA.AI.RAG.App.Domain.csproj", "src/KBA.AI.RAG.App.Domain/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/KBA.AI.RAG.App.Blazor"
RUN dotnet publish -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/KBA.AI.RAG.App.Blazor/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
