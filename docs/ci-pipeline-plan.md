# CI/CD Pipeline Plan

> **This document describes the conceptual CI/CD pipeline. No YAML is generated.**

---

## Pipeline 1 — `ci.yml` (Continuous Integration)

### Trigger

Runs on every **Pull Request** targeting:

- `main`
- `dev`

This ensures all code changes are validated before merging.

### Pipeline Structure

The pipeline consists of **two parallel jobs**: one for the backend and one for the frontend.

#### Job 1 — Backend (.NET 8)

| Step                     | What it does                                                    |
| ------------------------ | --------------------------------------------------------------- |
| **Checkout**             | Pull the latest code from the PR branch                         |
| **Setup .NET 8 SDK**     | Install the .NET 8 SDK on the runner                            |
| **Restore dependencies** | Run `dotnet restore` to fetch NuGet packages                    |
| **Build**                | Run `dotnet build --no-restore` to compile the API project      |
| **Run tests**            | Run `dotnet test --no-build` — placeholder test project for now |

#### Job 2 — Frontend (Angular 17+)

| Step                     | What it does                                        |
| ------------------------ | --------------------------------------------------- |
| **Checkout**             | Pull the latest code from the PR branch             |
| **Setup Node 18**        | Install Node.js 18 LTS on the runner                |
| **Install dependencies** | Run `npm ci` inside `frontend/` for a clean install |
| **Build**                | Run `npx ng build` to compile the Angular app       |

### Merge Policy

- **Both jobs must pass** before a PR can be merged
- Branch protection rules on `main` and `dev` should enforce this
- No manual overrides allowed

---

## Pipeline 2 — `deploy.yml` (Continuous Deployment)

### Trigger

Runs on every **push to `main`** only (i.e., after a PR is merged into `main`).

### Pipeline Structure

A single sequential workflow:

| Step | Action                                | Detail                                                       |
| ---- | ------------------------------------- | ------------------------------------------------------------ |
| 1    | **Checkout**                          | Pull the latest code from `main`                             |
| 2    | **Setup .NET 8 SDK**                  | Install .NET 8 on the runner                                 |
| 3    | **Restore + Build + Test** (backend)  | `dotnet restore` → `dotnet build` → `dotnet test`            |
| 4    | **Publish** (backend)                 | `dotnet publish -c Release -o ./publish`                     |
| 5    | **Setup Node 18**                     | Install Node.js 18 LTS on the runner                         |
| 6    | **Install + Build** (frontend)        | `npm ci` → `npx ng build --configuration=production`         |
| 7    | **Deploy API to App Service**         | Use `azure/webapps-deploy` action with publish profile       |
| 8    | **Deploy Angular to Static Web Apps** | Or deploy to App Service — whichever hosting model is chosen |

### GitHub Secrets Required

| Secret Name                       | Value                                            |
| --------------------------------- | ------------------------------------------------ |
| `AZURE_WEBAPP_PUBLISH_PROFILE`    | Azure App Service publish profile XML            |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Azure Static Web Apps deployment token (if used) |

> **Never hardcode credentials.** Store all Azure publish profiles and tokens as GitHub Secrets.

---

## Branch Protection Rules

Configure on GitHub for both `main` and `dev`:

| Rule                              | Setting |
| --------------------------------- | ------- |
| Require pull request reviews      | ✅ (1+) |
| Require status checks to pass     | ✅ (CI) |
| Require branches to be up-to-date | ✅      |
| Disable force pushes              | ✅      |
| Disable direct pushes             | ✅      |

---

## Expected Outcome

By Day 2, the team should be able to:

1. Merge a PR to `dev` → CI validates the build
2. Merge `dev` → `main` via PR → CD auto-deploys to Azure
3. See the change live within minutes

This is how professional teams work. Every merge to `main` results in a live deployment.

---

## Future Enhancements (Not Day 1)

- Linting (ESLint, dotnet-format)
- Code coverage thresholds
- Docker image build & push to ACR (Day 4)
- Deployment to staging slot + swap (Day 4)
- Smoke tests after deployment
