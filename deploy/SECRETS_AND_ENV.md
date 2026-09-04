# Secrets and environment files

## What / why

| Environment | How you run | Where secrets live |
|-------------|-------------|-------------------|
| **Development** | Local IDE (no Docker) | `appsettings.Development.json` |
| **Local Docker demo** | Root `docker-compose.yml` | Explicit Staging values in compose (demo only) |
| **Staging** | `deploy/docker-compose.staging.yml` | `deploy/.env.staging` (gitignored) |
| **Production** | `deploy/docker-compose.production.yml` | `deploy/.env.production` (gitignored) |

Committed examples:

- `deploy/.env.staging.example`
- `deploy/.env.production.example`

Copy on the server, fill values, and never commit the real files.

```bash
cp deploy/.env.staging.example deploy/.env.staging
cp deploy/.env.production.example deploy/.env.production
```

## How (Docker env → config)

ASP.NET Core binds nested keys with `__`:

```text
ConnectionStrings__LoyaltyDb=Server=sqlserver;...
AppSettings__IsSwaggerActivated=true
MSSQL_SA_PASSWORD=...
```

Deploy compose loads env via `env_file` and `--env-file` (for Compose variable substitution such as the SQL SA password).

## Run Staging / Production

From the repository root:

```bash
docker compose -f deploy/docker-compose.staging.yml --env-file deploy/.env.staging up -d --build
docker compose -f deploy/docker-compose.production.yml --env-file deploy/.env.production up -d --build
```

## Do not

- Do not commit `deploy/.env.staging` or `deploy/.env.production`.
- Do not put Staging/Production secrets into committed `appsettings*.json`.
- Do not put `AppSettings` secrets or flags in the Dockerfile `ENV`.
- Do not use a Development `.env` unless you intentionally need one. Development uses appsettings.

Before Staging or Production, also implement CORS, health checks, rate limiting, and related API security. See **Before Staging / Production** in [`README.md`](../README.md).
