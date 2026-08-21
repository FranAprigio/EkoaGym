# EkoaGym Backend

ASP.NET Core Web API backend for EkoaGym, organized as a clean architecture solution:

| Project | Responsibility |
|---|---|
| `src/EkoaGym.Domain` | Entities, no external dependencies |
| `src/EkoaGym.Application` | Services, DTOs, configuration options, interfaces |
| `src/EkoaGym.Infrastructure` | EF Core `DbContext`, migrations, data access |
| `src/EkoaGym.Api` | Controllers, middleware, composition root (`Program.cs`) |
| `tests/EkoaGym.Application.Tests` | Unit tests: validation attributes, `PagedResult` |
| `tests/EkoaGym.Infrastructure.Tests` | Unit tests for services (`AuthService`, `WorkoutPlanService`, `ProgressionService`, `ExerciseProgressService`) against EF Core's InMemory provider |
| `tests/EkoaGym.Api.Tests` | Integration tests via `WebApplicationFactory<Program>`, hitting real endpoints end-to-end |

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

## Exercise library

- `GET /api/exercises` — public, no auth required. Filter with `?muscleGroup=`, `?equipment=`, `?difficulty=` (enum names) and `?q=` (name search); paginate with `?page=` (default 1) and `?pageSize=` (default 20, max 100).
- `GET /api/exercises/{id}` — public, returns full detail (instructions + image URL) or 404 if unknown.
- The starter dataset (42 exercises, seeded via the `AddExercisesWithSeed` migration) is **original content written for this project**, not copied from WGER, ExerciseDB, or any other third-party source — so there are no external license terms to track for it. `ImageUrl` values are placeholders following the `/uploads/exercises/<slug>.gif` convention; real media still needs to be uploaded to that path (or swapped for real blob storage/CDN URLs) before going live. If a licensed third-party dataset (e.g. WGER, CC BY-SA 4.0) is imported later, record its license and attribution requirements here.

## Workout plans

- `GET/POST/PUT/DELETE /api/plans` and `POST /api/plans/{id}/duplicate` — all scoped to the authenticated user. Unlike Goals/Profile (which 404 on another user's resource), attempting to read/modify **another user's plan returns 403** per this epic's acceptance criteria — an intentional inconsistency across epics, not a bug.
- `POST`/`PUT` body: `{ name, exercises: [{ exerciseId, sets, reps, weightKg, restSeconds }, ...] }`. Exercise order is taken from array position (no separate `orderIndex` field to send); every `exerciseId` must reference an existing exercise (404 otherwise). `PUT` fully replaces the exercise list.
- `GET /api/plans/{planId}/exercises/{exerciseId}/next-target` suggests the next session's `Sets`/`Reps`/`WeightKg` (progressive overload): if every set in the most recently *completed* session met or exceeded the planned reps/weight, it suggests `+Progression:WeightIncrementKg` (default 2.5kg, configurable in appsettings); otherwise it repeats the plan's current values. Returns the plan's own values if there's no completed session history yet.

## Workout sessions

- `POST /api/sessions` — `{ planId }` → starts a session, returns `{ sessionId, planId, startedAt, exercises }` (the plan's exercise list to log against). 404/403 follow the same plan-ownership rules as `/api/plans`.
- `POST /api/sessions/{id}/sets` — `{ exerciseId, setNumber, actualReps, actualWeightKg }` → appends a set log. 409 if the session is already completed; 404 if `exerciseId` isn't part of the session's plan.
- `PUT /api/sessions/{id}/complete` — sets `EndedAt` (idempotent: calling it again on an already-completed session just returns the existing completion data, no error).
- `GET /api/sessions` — paginated (`?page=`, `?pageSize=`), filterable by `?from=`/`?to=` (matched against `StartedAt`); returns summaries (plan name, start/end, duration, distinct exercise count, total volume = Σ actualReps × actualWeightKg).
- `GET /api/sessions/{id}` — full set-by-set detail.
- Session ownership uses **404** (not the 403 used by `/api/plans`) for another user's session — no 403 was requested for this epic, so it follows the more conservative "hide existence" default used by Goals/Profile instead.
- This epic formalizes `WorkoutSession` as a real entity and reshapes `SessionSetLog` (added as a placeholder in the Workout Plans epic) to reference it via `SessionId` instead of loose `UserId`/`PlanId` columns, with `ActualReps`/`ActualWeightKg` field names — see migration `AddWorkoutSessions`.

## Progress tracking

- `GET/POST /api/body-metrics` — `{ date, weightKg, waistCm?, chestCm?, armsCm? }`. `GET` supports `?from=`/`?to=` (date range) and always returns entries **ascending by date** (oldest first, suited for charting). No update/delete — only create/list were requested.
- `POST /api/progress-photos` — `multipart/form-data` with a `File` (JPEG/PNG/WEBP only, 10MB max) and a `Date` field. `GET /api/progress-photos` returns photos **descending by date** (most recent first, suited for a gallery/timeline).
  - **Security note**: progress photos are private body photos, not public reference data like exercise images, so uploads are **not** served via a public static-file path. Each photo is fetched through an authenticated `GET /api/progress-photos/{id}/image` endpoint that verifies the requesting user owns the photo before streaming it from disk (404 otherwise). Filenames on disk are derived only from the photo's own GUID + a whitelisted extension (never from client-supplied filenames), avoiding path traversal.
  - `ProgressPhoto` has a `ContentType` column beyond the original tech note — needed to know what MIME type/extension to serve back through the streaming endpoint.
- `GET /api/exercises/{exerciseId}/progress` — time-ordered series (one point per session that logged the exercise), each with `bestSetReps`/`bestSetWeightKg` (the set with the highest weight×reps that session), `totalVolumeKg`, `totalSets`, `totalReps`, and `isWeightPR`/`isRepPR` flags computed chronologically against all prior sessions (no PR table — computed at query time). Supports `?from=`/`?to=` (matched against session start). 404 if the exercise doesn't exist; empty array (not an error) if the user never logged it.
  - This lives in its own `ExerciseProgressController` (`[Authorize]`), not inside `ExercisesController` — that controller is `[AllowAnonymous]`, and ASP.NET Core's `[AllowAnonymous]` on a controller cannot be overridden by `[Authorize]` on one of its actions (it short-circuits auth checks for the whole endpoint regardless).

## Testing

Run the whole suite from the repo root:

```powershell
dotnet test
```

Or a single project (faster while iterating):

```powershell
dotnet test tests/EkoaGym.Application.Tests
dotnet test tests/EkoaGym.Infrastructure.Tests
dotnet test tests/EkoaGym.Api.Tests
```

- **`EkoaGym.Application.Tests`** — pure unit tests for validation attributes (`PasswordComplexityAttribute`, `NotInFutureAttribute`, `NotInPastAttribute`) and `PagedResult`, plus a reflection-based regression test that fails the build if any record request DTO has a validation attribute targeting the generated property instead of the primary constructor parameter (see the "record validation attributes" gotcha below).
- **`EkoaGym.Infrastructure.Tests`** — exercises the real service classes (`AuthService`, `WorkoutPlanService`, `ProgressionService`, `ExerciseProgressService`) against EF Core's InMemory provider (via `TestDbContextFactory`, one fresh isolated database per test).
- **`EkoaGym.Api.Tests`** — full HTTP integration tests via `WebApplicationFactory<Program>` (`ApiTestFactory`), which swaps the real SQL Server `DbContext` for an isolated InMemory one and supplies deterministic JWT/Progression config, so tests never depend on a developer's local `.env`/`appsettings.Development.json`. Includes regression tests for three real bugs found during manual testing (see below).

None of this requires Docker or the local dev SQL Server container to be running — all three test projects are fully self-contained.

### Bugs found only via runtime testing (not caught by `dotnet build`)

- **Record validation attributes**: `[property: Required]` etc. on a record's primary-constructor parameter throws `InvalidOperationException` at request time in ASP.NET Core (it validates via the constructor parameter, not the property). Fixed by using bare attributes (e.g. `[Required] string Email`) with no `property:` target. Guarded going forward by `RecordValidationAttributeTargetTests`.
- **Enum JSON (de)serialization**: enums default to raw ints in JSON; fixed globally via `JsonStringEnumConverter` registered on `AddControllers().AddJsonOptions(...)`. Regression-covered by `ProfileEndpointsTests`.
- **`DateTime` losing UTC `Kind` on reload from SQL Server**: fixed via a `ConfigureConventions` value converter in `EkoaGymDbContext` forcing `DateTimeKind.Utc` on read (no migration needed — it's a CLR-side behavior change only). Regression-covered by `SessionsRegressionTests`.
- **JWT bearer options read config too eagerly**: `Program.cs` originally captured `JwtOptions` into a plain local variable via `builder.Configuration.GetSection(...).Get<JwtOptions>()` *before* `builder.Build()`. This works in production (config is fully loaded synchronously by then) but breaks under `WebApplicationFactory`, whose test-only config overrides only apply during `Build()` — the JWT signing key ended up empty in tests. Fixed by resolving `JwtOptions` lazily via `services.AddOptions<JwtBearerOptions>(...).Configure<IOptions<JwtOptions>>(...)` instead of a pre-`Build()` snapshot.

