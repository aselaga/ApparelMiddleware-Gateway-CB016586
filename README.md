# Apparel Middleware Gateway

A .NET 8 Web API that acts as a **zero-trust middleware gateway** between a Low-Code/No-Code
(LCNC) operator frontend and an apparel-manufacturing ERP. It receives fabric **spreader
telemetry** (ply height, end bit, overlap, damage) captured per fabric roll against a cutting
job, validates it, and persists it to SQL Server.

MSc dissertation artefact.

## Architecture

```
LCNC tablet UI  ──HTTPS/JWT──▶  ApparelMiddlewareGateway  ──▶  SQL Server (SpreaderERPDB)
                                        │
                                        └── (production) Azure Key Vault via Managed Identity
```

| Concern | Where |
|---|---|
| Pipeline / DI wiring, config validation, rate limiter | `Program.cs` |
| Global exception handling (RFC 7807) | `Infrastructure/GlobalExceptionHandler.cs` |
| Gated test-token minting | `Controllers/AuthController.cs` |
| Cutting-job lookup + telemetry ingest | `Controllers/TelemetryController.cs` |
| Persistence model, relationships, seed data | `Models/`, `Data/AppDbContext.cs` |

> **Thesis alignment:** `Thesis-Chapter4-5-Updates.md` (in the `Research/` folder) maps every
> code change here to paste-ready replacement text for Chapters 4 and 5 of the dissertation.
> The Azure demo slot runs as **Production** (so Key Vault + Swagger-off behave normally) with
> `Auth__EnableTestTokenEndpoint=true` so the evaluation harness can still obtain a token.

## Test-token endpoint

`GET /api/Auth/generate-test-token` mints a valid 30-minute operator JWT **without
authentication** — a test affordance, not part of the security model. It is reachable only
in the Development environment, or when `Auth:EnableTestTokenEndpoint` is explicitly `true`
(a `SECURITY:` warning is logged at start-up if that happens outside Development). A real
deployment leaves it off and issues tokens from an enterprise OIDC provider.
Optional `?operator=` and `?line=` query parameters override the defaults
(`OP-777` / `Spreader_01`) — e.g. `?operator=OP-901&line=Spreader_02` for the positive-case
Spreader_02 demonstration.

## Data model

- `CuttingJob` (`CuttingJobNo` PK, `TargetLine`, `Status`) — one job has many `Roll`s.
- `Roll` (composite key `CuttingJobNo` + `RollID`, `Status` = `Pending` → `Processed`), FK to
  `CuttingJob` with cascade delete.
- `SpreaderTelemetry` — one row per processed roll; unique index on `(CuttingJobNo, RollID)`.
- `GET /api/Telemetry/cutting-job/{jobNo}` returns the job and its rolls from the database
  (`404` if unknown, `403` if the job is not on the operator's line). Seeds:
  `CJ-9920` (Spreader_01, rolls R-1045/46/47) and `CJ-9921` (Spreader_02, rolls R-2050/51).
- On a successful telemetry submission the matching roll is flipped to `Processed` in the same
  transaction.

## Security controls

- **Authentication** — JWT bearer, validating issuer, audience, lifetime and signing key
  (HMAC-SHA256, 30s clock skew).
- **Authorisation (RBAC)** — an operator may only read or submit against cutting jobs whose
  `TargetLine` (from the database) matches their `AssignedLine` token claim. The decision never
  uses a caller-supplied value such as the `lineID` query parameter, so a caller cannot widen
  their own access by editing the request. Denials are logged and return `403`.
- **Referential validation** — a submission is rejected if the cutting job is unknown (`404`),
  the supplied `lineID` is inconsistent with the job's line (`400` — client/UI state is stale),
  or the roll is not assigned to that job (`400`).
- **Fail-fast configuration** — the app refuses to start if the JWT secret / issuer / audience
  or the connection string is missing, or if the signing key is shorter than 256 bits.
- **Secret management** — no secrets in `appsettings.json`; development secrets live in
  `appsettings.Development.json`, production secrets come from Azure Key Vault.
- **Rate limiting** — fixed-window, partitioned per operator (falling back to remote IP);
  `RateLimiting:PermitLimit` / `RateLimiting:WindowSeconds` (default 100 / 60s), `429` on breach.
  Raise the limit or use one token per virtual user when load-testing with JMeter.
- **Input validation** — data-annotation constraints on the request DTO (`[ApiController]`
  auto-returns `400` with a validation problem-details body).
- **Integrity** — a unique index on `(CuttingJobNo, RollID)` enforces "one roll per job" at the
  database level; the API returns `409 Conflict` for duplicates, including under a race.
- **Error hygiene** — unhandled exceptions are logged server-side and returned to the client as
  a generic problem-details `500` with no internal detail.
- **Swagger** — served only in the Development environment.
- **CORS** — origin allowlist from configuration (`Cors:AllowedOrigins`).
- **Auditability** — token issuance, RBAC denials, duplicates and successful writes are logged.

## Running locally

Requires the .NET 8 SDK and SQL Server LocalDB.

```bash
cd ApparelMiddlewareGateway
dotnet tool restore
dotnet dotnet-ef database update
dotnet run
```

Swagger UI: `https://localhost:7239/swagger`. `ApparelMiddlewareGateway.http` contains a full
request walkthrough (token → job lookup → submit → 409 → 403).

## Configuration keys

| Key | Dev source | Production source |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `appsettings.Development.json` | Key Vault |
| `Jwt:Key` | `appsettings.Development.json` | Key Vault |
| `Jwt:Issuer`, `Jwt:Audience` | `appsettings.json` | `appsettings.json` / Key Vault |
| `KeyVault:VaultUri` | – | environment / `appsettings.Production.json` |
| `Cors:AllowedOrigins` | `appsettings.json` | `appsettings.json` / Key Vault |
| `RateLimiting:PermitLimit` / `WindowSeconds` | `appsettings.json` (100 / 60) | `appsettings.json` / env |
| `Auth:EnableTestTokenEndpoint` | implicit (Development) | `false` — set `true` only on the demo slot |
