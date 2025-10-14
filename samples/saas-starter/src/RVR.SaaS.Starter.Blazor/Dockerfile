# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["RVR.SaaS.Starter.sln", "./"]
COPY ["src/RVR.SaaS.Starter.Blazor/RVR.SaaS.Starter.Blazor.csproj", "src/RVR.SaaS.Starter.Blazor/"]
COPY ["src/RVR.SaaS.Starter.Application/RVR.SaaS.Starter.Application.csproj", "src/RVR.SaaS.Starter.Application/"]
COPY ["src/RVR.SaaS.Starter.Domain/RVR.SaaS.Starter.Domain.csproj", "src/RVR.SaaS.Starter.Domain/"]

RUN dotnet restore "src/RVR.SaaS.Starter.Blazor/RVR.SaaS.Starter.Blazor.csproj"

COPY . .

WORKDIR "/src/src/RVR.SaaS.Starter.Blazor"
RUN dotnet publish "RVR.SaaS.Starter.Blazor.csproj" -c Release -o /app/publish

# Serve with nginx
FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
