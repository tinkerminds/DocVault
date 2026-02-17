# CI Pipeline Plan — `ci.yml` Intent

> **This document describes the conceptual CI pipeline. No YAML is generated.**

---

## Trigger

The pipeline runs on every **Pull Request** targeting:

- `main`
- `dev`

This ensures all code changes are validated before merging.

---

## Pipeline Structure

The pipeline consists of **two parallel jobs**: one for the backend and one for the frontend.

### Job 1 — Backend (.NET 8)

| Step                     | What it does                                                    |
| ------------------------ | --------------------------------------------------------------- |
| **Checkout**             | Pull the latest code from the PR branch                         |
| **Setup .NET 8 SDK**     | Install the .NET 8 SDK on the runner                            |
| **Restore dependencies** | Run `dotnet restore` to fetch NuGet packages                    |
| **Build**                | Run `dotnet build --no-restore` to compile the API project      |
| **Run tests**            | Run `dotnet test --no-build` — placeholder test project for now |

### Job 2 — Frontend (Angular 17+)

| Step                     | What it does                                        |
| ------------------------ | --------------------------------------------------- |
| **Checkout**             | Pull the latest code from the PR branch             |
| **Setup Node 18**        | Install Node.js 18 LTS on the runner                |
| **Install dependencies** | Run `npm ci` inside `frontend/` for a clean install |
| **Build**                | Run `npx ng build` to compile the Angular app       |

---

## Merge Policy

- **Both jobs must pass** before a PR can be merged
- Branch protection rules on `main` and `dev` should enforce this
- No manual overrides allowed during Day 1

---

## Future Enhancements (Not Day 1)

- Linting (ESLint, dotnet-format)
- Code coverage thresholds
- Docker image build & push
- Deployment to Azure staging
