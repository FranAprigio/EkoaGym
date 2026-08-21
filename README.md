# EkoaGym Backend

ASP.NET Core Web API backend for EkoaGym, organized as a clean architecture solution:

| Project | Responsibility |
|---|---|
| `src/EkoaGym.Domain` | Entities, no external dependencies |
| `src/EkoaGym.Application` | Services, DTOs, configuration options, interfaces |
| `src/EkoaGym.Infrastructure` | EF Core `DbContext`, migrations, data access |
| `src/EkoaGym.Api` | Controllers, middleware, composition root (`Program.cs`) |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/), set to **Linux containers** (required — SQL Server's Linux image does not run on Windows containers)
  - Allocate at least **2GB RAM** to Docker Desktop for the SQL Server container

## Primary workflow: Docker Compose

1. Copy the environment template and fill in real values:

   ```powershell
   cp .env.example .env
   ```

2. Start the full backend (API + SQL Server):

   ```powershell
   docker-compose up --build
   ```

   The API is available at `http://localhost:8080`, with Swagger UI at `http://localhost:8080/swagger` when `ASPNETCORE_ENVIRONMENT=Development`.

3. Apply database migrations against the containerized SQL Server (from the host, with the stack running):

   ```powershell
   dotnet ef database update --project src/EkoaGym.Infrastructure --startup-project src/EkoaGym.Api --connection "Server=localhost,1433;Database=EkoaGym;User Id=sa;Password=<SA_PASSWORD from .env>;TrustServerCertificate=True;"
   ```

4. Stop the stack. Data persists across restarts via named volumes (`mssql_data`, `api_uploads`):

   ```powershell
   docker-compose down
   ```

## Alternate workflow: local `dotnet run`

1. Copy `src/EkoaGym.Api/appsettings.Development.json.example` to `src/EkoaGym.Api/appsettings.Development.json` and fill in a local connection string and JWT values (this file is gitignored).
2. Have a SQL Server instance reachable at the connection string you configured.
3. Run the API:

   ```powershell
   dotnet run --project src/EkoaGym.Api
   ```

## EF Core migrations

Install the tool once, if you don't already have it:

```powershell
dotnet tool install --global dotnet-ef
```

Create a new migration after changing `Domain` entities:

```powershell
dotnet ef migrations add <MigrationName> --project src/EkoaGym.Infrastructure --startup-project src/EkoaGym.Api
```

Apply pending migrations to the database configured in `ConnectionStrings:DefaultConnection`:

```powershell
dotnet ef database update --project src/EkoaGym.Infrastructure --startup-project src/EkoaGym.Api
```

## Configuration

Secrets (connection strings, JWT signing key/issuer/audience) are never hardcoded:

- Local `dotnet run`: `src/EkoaGym.Api/appsettings.Development.json` (gitignored; copy from the committed `.example` file)
- Docker: `.env` (gitignored; copy from the committed `.env.example`), mapped to config via double-underscore env vars (e.g. `ConnectionStrings__DefaultConnection`, `Jwt__SigningKey`)

CORS allowed origins for the mobile app are configured under `Cors:AllowedOrigins` in appsettings. If left empty in Development, the API allows any origin as a local convenience fallback.

## Authentication

- `POST /api/auth/register` — `{ email, password, name }` → 200 with `{ accessToken, refreshToken, accessTokenExpiresAt }`, 409 if the email is already registered.
- `POST /api/auth/login` — `{ email, password }` → 200 with tokens, 401 on invalid credentials.
- `POST /api/auth/refresh` — `{ refreshToken }` → 200 with a new token pair (refresh tokens are rotated/single-use), 401 if invalid/expired/revoked.
- All other controllers must add `[Authorize]` explicitly (see `AuthController` for the `[AllowAnonymous]` exception); requests without a valid JWT bearer token get 401.

