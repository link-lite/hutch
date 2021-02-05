# Link Lite

![Build and Test](https://github.com/biobankinguk/link-lite/workflows/Build%20and%20Test/badge.svg)

# Configuration

```json
"ConnectionStrings": {
  // Default is the details for the local development db in docker
  "Omop": "Server=localhost;Username=postgres;Password=example;Database=omop"
},
"Serilog": {
    //...
},
"QueryPollingInterval": 5, // seconds
"RquestCollectionId": "",
"RquestConnectorApi": {
    "BaseUrl": "", // for the Connector API specifically, not just general RQUEST URL, e.g. `https://rquestserver.com/task/capi/`,
    "QueueStatusEndpoint": "queue",
    "FetchQueryEndpoint": "query",
    "SubmitResultEndpoint": "result",
},
```

# App requirements

- .NET 5 SDK

# OMOP

## Postgres Instance

A `docker-compose` file is provided in `omop/` for dev environment use. It runs a postgres instance on the default port (`5432`) with the credentials specified, and the "adminer" GUI tool on port `8080`.

## Structure

This tool works with the OMOP Common Data Model `5.3.x`

https://github.com/OHDSI/CommonDataModel

Follow the instructions / use the scripts for Postgres.

Versioned script archives, with per platform guidance, can be downloaded from the GitHub Releases.

Notes:

- the `5.3.1` (latest `5.x` at time of writing) postgres DDL script is broken
  - find and replace `DATETIME2` with `TIMESTAMP` to fix.
  - https://github.com/OHDSI/CommonDataModel/issues/256
- When it says to populate data, if you have a dataset (see below), do so.
- ⚠ For now, **do not** add foreign key constraints (see below).

## Data

For proof of concept this project uses a test dataset (available from the project team). Because this dataset is all that is queried, there is no need to populate OMOP Vocabularies.

⚠ Because of this, you **should not** add the foreign key constraints to the schema.

### Test Data issues / notes

- `location` schema mismatch (test vs 5.3.1 script)
  - extra `country`, `longitude` and `latitude` columns
    - ignore them
- `person` schema mismatch (test vs 5.3.1 script)
  - extra `death_datetime` column
    - ignore it

### Future

As the app progresses beyond the proof of concept stage, the following OMOP Vocabulary data will be needed:

- SNOMED
- ICD9
- ICD10
- READ
- LOINC

The vocabulary data can be acquired [here](https://athena.ohdsi.org/vocabulary/list) with a free account.

As the app is productionised, guidance for loading organisation data (initial data at point of installation, as well as updates) will be formed, and possibly additional tooling or services (such as a local API facade) provided.
