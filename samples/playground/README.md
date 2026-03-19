# RIVORA Playground

A minimal sample API to quickly explore and test the RIVORA Framework.

## Prerequisites

- .NET 9 SDK

## How to Run

```bash
# From the repository root
dotnet run --project samples/playground

# Or from this directory
dotnet run
```

The API starts on `http://localhost:5000` by default.

## Available Endpoints

| Method | Path              | Description                          |
|--------|-------------------|--------------------------------------|
| GET    | `/api/hello`      | Returns a greeting message           |
| GET    | `/api/hello/{name}` | Returns a personalized greeting    |
| GET    | `/api/health`     | Returns framework health status      |
| GET    | `/healthz`        | ASP.NET Core health check endpoint   |
| GET    | `/swagger`        | Swagger UI (development mode only)   |

## Quick Test

```bash
curl http://localhost:5000/api/hello
curl http://localhost:5000/api/hello/World
curl http://localhost:5000/api/health
```

## Documentation

For full RIVORA Framework documentation, visit the [project wiki](../../docs) or the official website.
