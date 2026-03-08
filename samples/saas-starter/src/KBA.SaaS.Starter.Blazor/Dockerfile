# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["KBA.SaaS.Starter.sln", "./"]
COPY ["src/KBA.SaaS.Starter.Blazor/KBA.SaaS.Starter.Blazor.csproj", "src/KBA.SaaS.Starter.Blazor/"]
COPY ["src/KBA.SaaS.Starter.Application/KBA.SaaS.Starter.Application.csproj", "src/KBA.SaaS.Starter.Application/"]
COPY ["src/KBA.SaaS.Starter.Domain/KBA.SaaS.Starter.Domain.csproj", "src/KBA.SaaS.Starter.Domain/"]

RUN dotnet restore "src/KBA.SaaS.Starter.Blazor/KBA.SaaS.Starter.Blazor.csproj"

COPY . .

WORKDIR "/src/src/KBA.SaaS.Starter.Blazor"
RUN dotnet publish "KBA.SaaS.Starter.Blazor.csproj" -c Release -o /app/publish

# Serve with nginx
FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
