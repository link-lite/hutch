# HUTCH

HUTCH is an application that allows federated access to summary statistics of medical data at an institution.

It consists of a Web application (React frontend, .NET backend API, SQL DB) for users to interact with, and Python-based agents which retrieve the data.

# đŠâđģ Getting Started

## Prerequisites

1. **.NET SDK** `6.x`
  - The backend API is .NET6 (LTS)
1. **Node.js** `>=16.9`
  - `16.9` and newer include **Corepack**
  - `16.x` is LTS at time of writing
1. **Enable [Corepack](https://nodejs.org/api/corepack.html)**
  - Simply run `corepack enable` in your cli
1. PostgreSQL DB
1. RabbitMQ instance

> âšī¸
> 
> The provided `docker-compose.yml` provides suitable Postgres and RabbitMQ development instances.

## Database setup

The application stack interacts with a PostgreSQL database, and uses code-first migrations for managing the database schema.

When setting up a new environment, or running a newer version of the codebase if there have been schema changes, you need to run migrations against your database server.

The easiest way is using the dotnet cli:

1. If you haven't already, install the local Entity Framework tooling

- Anywhere in the repo: `dotnet tool restore`

1. Navigate to the same directory as `HutchManager.csproj`
1. Run migrations:

- `dotnet ef database update`
- The above runs against the default local server, using the connection string in `appsettings.Development.json`
- You can specify a connection string with the `--connection "<connection string>"` option

## Working with JavaScript

This monorepo uses [pnpm](https://pnpm.io) workspaces to manage JS dependencies and scripts.

Basically, where you might normally use `npm` or `yarn`, please use `pnpm` commands instead.

You don't need to install anything special; Corepack will.

A brief [pnpm cheatsheet](#-pnpm-cheatsheet) is provided later in this document.

# đ Repository contents

Areas within this repo include:

- Application Source Code
  - .NET6 backend API
  - React (Vite) frontend client app
  - Python agent
- GitHub Actions
  - workflows for building and deploying the applications
# đĄ pnpm cheatsheet

Most pnpm commands can be done recursively against all workspaces with `-r`

You can target a specific workspace by being inside its workspace directory

- or you can target a workspace by relative directory path `-C <dir>`
- or you can filter workspaces to target using `--filter <filter-spec>`
  - See the docs for more complex filtering than just package name

## Dependency management

To install current dependencies for the whole repo: `pnpm i`

> âš
>
> pnpm symlinks `node_modules` inside workspaces.
>
> If you need to clean out `node_modules` you can't just do the root one, so use `pnpm dlx npkill` which will let you delete them all :)

To add a new dependency `pnpm add <package-name>` with `-D` if you want it to be a dev dependency

For information on dependency management in the Python agent, please read the README, [here](app/HutchAgent/README.md).

## Script running

Run scripts with `pnpm <script-name>`

> âš
>
> If the name of the script conflicts with a pnpm command, do `pnpm run <script-name>`

# App Configuration

Notes on configuration values that can be provided, and their defaults.

## Manager

The app can be configured in any standard way an ASP.NET Core application can. Typically from the Azure Portal (Environment variables) or an `appsettings.json`.

```yaml
ConnectionStrings:
  Default: "" # the main application SQL Server database
Serilog:
  # ...
OutboundEmail:
  ServiceName: Hutch
  FromName: No Reply
  FromAddress: noreply@example.com
  ReplyToAddress: ""
  Provider: local

  # If Provider == "local"
  LocalPath: ~/temp

  # If Provider == "sendgrid"
  SendGridApiKey: ""

ActivitySourcePolling:
  PollingInterval: 5 # set to a negative value will disable polling altogether

RquestTaskApi:
  BaseEndpoint: "bcos-rest/api/task"
  QueueStatusEndpoint: "queue"
  FetchQueryEndpoint: "nextjob"
  SubmitResultEndpoint: "result"
  Username: ""
  Password: ""

JobQueue:
  HostName: ""
  Port: 5672
  UserName: "guest"
  Password: "guest"

# Opt in feature flags
# sometimes features here are works in progress
FeatureManagement:
  UseROCrates: false # WIP
  AllowFreeRegistration: false # By default, the app uses an Allowlist for new account registration; setting this to `true` bypasses that.
```

## Agent

The agent is configured by environment variables; for development it will load a `.env` file local to `pyproject.toml`.

Example .env for development (also documents unnecessary/default values)

```sh
# Logging Database configuration

# LOG_DB_DRIVERNAME="postgresql" # SQLAlchemy driver names (including short names). See currently supported list.
LOG_DB_HOST="localhost"
# LOG_DB_PORT=<driver default> # will use the default port of the database driver
LOG_DB_DATABASE="postgres"
LOG_DB_USERNAME="postgres"
LOG_DB_PASSWORD="example"


# Data source configuration

DATASOURCE_NAME="jobs"
# DATASOURCE_DB_DRIVERNAME="postgresql"
DATASOURCE_DB_HOST=""
# DATASOURCE_DB_PORT=<driver default> # will use the default port of the database driver
DATASOURCE_DB_DATABASE=""
DATASOURCE_DB_USERNAME=""
DATASOURCE_DB_PASSWORD=""
# DATASOURCE_DB_SCHEMA=<driver default> # will use the default schema of the database driver (e.g. `public` for postgres, `dbo` for MSSQL...)


# Manager related configuration

MANAGER_URL="https://localhost:45588"
MANAGER_VERIFY_SSL=0 # Disable SSL verification ONLY IN DEVELOPMENT to allow for self-signed certs. Actual in-app default is 1.

# Check In schedule

# CHECKIN_CRON="0 */1 * * *" # once every hour


# Feature Flags

# USE_RO_CRATES=0 # Use RO CRATES Query and Results Schema internally, instead of Rquest
# USE_RESULTS_MODS=0 # Whether to run results modifiers or not when executing a query
```

### Currently supported SQLAlchemy drivers

We currently only depend on `psycopg2` so that's the only supported driver at this time.

Valid values:

- `postgresql`
- `postgresql+psycopg2`
