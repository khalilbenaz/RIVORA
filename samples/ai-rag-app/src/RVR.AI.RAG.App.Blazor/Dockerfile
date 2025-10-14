FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RVR.AI.RAG.App.sln", "./"]
COPY ["src/RVR.AI.RAG.App.Blazor/RVR.AI.RAG.App.Blazor.csproj", "src/RVR.AI.RAG.App.Blazor/"]
COPY ["src/RVR.AI.RAG.App.Application/RVR.AI.RAG.App.Application.csproj", "src/RVR.AI.RAG.App.Application/"]
COPY ["src/RVR.AI.RAG.App.Domain/RVR.AI.RAG.App.Domain.csproj", "src/RVR.AI.RAG.App.Domain/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/RVR.AI.RAG.App.Blazor"
RUN dotnet publish -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/RVR.AI.RAG.App.Blazor/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
