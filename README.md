# DocVault

**Secure Document Management Platform** — AZ-204 Capstone Project

DocVault is a document management app where employees can upload, search, and download files. We built it using Angular for the frontend and .NET 8 for the backend, and connected it with a bunch of Azure services to learn how they all work together in a real project.

> This is a team project by freshers who just completed AZ-204 training. The focus is on wiring Azure services together and understanding why each one exists — not on making a pixel-perfect UI.

---

## Tech Stack

- **Frontend**: Angular 19 (standalone components, Angular Material)
- **Backend**: .NET 8 Web API
- **Cloud**: Microsoft Azure (App Service, Blob Storage, Cosmos DB, Functions, and more)
- **CI/CD**: GitHub Actions
- **Auth**: Microsoft Entra ID (MSAL.js)

---

## Repo Structure

```
DocVault/
├── frontend/               # Angular SPA
├── backend/                # .NET 8 Web API
│   └── DocVault.Api/
├── functions/              # Azure Functions (blob trigger, service bus trigger)
├── infra/                  # Azure CLI setup scripts
├── docs/                   # Architecture docs, data models, team notes
└── .github/workflows/      # CI/CD pipeline (ci.yml + deploy.yml)
```

---

## How to Run Locally

```bash
# clone the repo
git clone https://github.com/tinkerminds/DocVault.git
cd DocVault

# frontend
cd frontend
npm install
ng serve
# runs on http://localhost:4200

# backend (in a new terminal)
cd backend/DocVault.Api
dotnet restore
dotnet run
# runs on https://localhost:7xxx
```

---

## Git Branching Strategy

We follow a strict branching model. **No one pushes directly to `main` or `dev` — everything goes through Pull Requests.**

```
feature/upload-api  ──PR──►  dev  ──PR (end of day)──►  main  ──auto──►  Azure
feature/auth        ──PR──┘                                              🚀
feature/thumbnails  ──PR──┘
```

| Branch      | What it's for                                    | Rules                                                |
| ----------- | ------------------------------------------------ | ---------------------------------------------------- |
| `main`      | Production code, auto-deploys to Azure           | Protected — no direct pushes, PRs only, CI must pass |
| `dev`       | Integration branch where we test things together | Protected — no direct pushes, PRs only, CI must pass |
| `feature/*` | One branch per task (not per person)             | Branch off `dev`, merge back to `dev` via PR         |

### Why we do it this way

- **No direct pushes** = every change gets reviewed by at least one teammate before merging
- **CI runs on every PR** = if the build breaks, we catch it before it gets merged
- **`dev` → `main` only when stable** = main always has working code that can deploy

---

## CI/CD Pipeline

We have two GitHub Actions workflows set up:

### `ci.yml` — Runs on every PR to `dev` and `main`

What it does:

1. Checks out the code
2. Builds and tests the .NET API (`dotnet restore` → `build`)
3. Builds and tests the Angular app (`npm ci` → `build` → `test`)
4. Reports pass/fail status back to the PR

**If CI fails, the PR cannot be merged.** We fix the issue on our feature branch first.

### `deploy.yml` — Runs when code lands on `main`

What it does:

1. Runs all CI checks again (frontend + backend build & test)
2. Publishes the .NET API and deploys it to **Azure App Service**
3. Builds the Angular app for production and deploys to **Azure Static Web Apps**

Credentials are stored as **GitHub Secrets** — we never hardcode any Azure keys or publish profiles in the code.

---

## Commit Convention

We use this format: `type(scope): description`

```
feat(api): add upload endpoint with blob storage
fix(auth): correct MSAL redirect URI
chore(infra): add Key Vault resource to setup script
ci(deploy): add Azure App Service deployment step
docs(readme): update branching strategy section
```

| Type       | When to use                       |
| ---------- | --------------------------------- |
| `feat`     | New feature                       |
| `fix`      | Bug fix                           |
| `docs`     | Documentation changes             |
| `chore`    | Config, dependencies, maintenance |
| `ci`       | CI/CD pipeline changes            |
| `refactor` | Code cleanup (no behavior change) |
| `test`     | Adding or updating tests          |

---

## PR Rules

Every change — even small ones — goes through a Pull Request. Here's what we follow:

1. **At least one teammate must review and approve** before merging
2. **PR description must answer**: What does this change? Which Azure service does it touch? How to test it?
3. **Keep PRs small** — if a feature is big, split it (e.g., API first, then UI, then wiring)
4. **Fix merge conflicts on your branch**, never on `dev` or `main`
5. **If CI is red, fix it immediately** — a broken pipeline blocks everyone

---

## Azure Services We Used (and Why)

| Service              | Why we used it                                                           | Day     |
| -------------------- | ------------------------------------------------------------------------ | ------- |
| App Service          | Host the .NET API                                                        | Day 1-2 |
| Static Web Apps      | Host the Angular SPA                                                     | Day 1-2 |
| Blob Storage         | Store uploaded documents + thumbnails                                    | Day 1   |
| Cosmos DB            | Store document metadata, tags, audit logs                                | Day 1   |
| SAS Tokens           | Time-limited download links (not direct blob URLs)                       | Day 1   |
| Lifecycle Policy     | Auto-move old docs to Cool (30d) / Archive (180d) storage                | Day 1   |
| Azure Functions      | Blob-triggered thumbnail generation + text extraction                    | Day 2   |
| Microsoft Entra ID   | User login via MSAL, API protected with JWT tokens                       | Day 2   |
| Key Vault            | Store all connection strings and secrets securely                        | Day 2   |
| Managed Identity     | Let App Service + Functions access Key Vault without any secrets in code | Day 2   |
| Event Grid           | Publish DocumentUploaded event, decouple upload from processing          | Day 3   |
| Service Bus          | Queue processing jobs for reliable background work                       | Day 3   |
| API Management       | Rate-limiting, CORS, caching policies in front of the API                | Day 3   |
| Application Insights | Telemetry, custom metrics, availability tests                            | Day 3   |
| Container Registry   | Store Docker images                                                      | Day 4   |
| Container Apps       | Deploy containerized API with scale-to-zero                              | Day 4   |

---

## API Endpoints

| Method | Route                      | What it does                                          |
| ------ | -------------------------- | ----------------------------------------------------- |
| POST   | `/api/documents`           | Upload a file (multipart/form-data)                   |
| GET    | `/api/documents`           | List current user's documents                         |
| GET    | `/api/documents/{id}`      | Get document metadata + SAS download URL              |
| DELETE | `/api/documents/{id}`      | Soft-delete a document                                |
| GET    | `/api/documents/search?q=` | Search documents by text content                      |
| GET    | `/api/health`              | Health check (used by App Insights availability test) |

---

## 4-Day Sprint Plan

| Day   | Focus                     | What we deliver                                                     |
| ----- | ------------------------- | ------------------------------------------------------------------- |
| Day 1 | Foundation & Storage      | Upload/download works, Blob + Cosmos DB, CI/CD pipeline is green    |
| Day 2 | Security & Serverless     | Entra ID login, Key Vault secrets, Functions auto-process uploads   |
| Day 3 | Events & Observability    | Event Grid, Service Bus, APIM policies, App Insights telemetry      |
| Day 4 | Containers, Polish & Demo | Docker + Container Apps, dashboard, architecture diagram, team demo |

---

## Prerequisites

| Tool        | Version |
| ----------- | ------- |
| Node.js     | 20+     |
| Angular CLI | 19+     |
| .NET SDK    | 8.0+    |
| Azure CLI   | 2.50+   |
| Git         | 2.40+   |

---

## Team

Built by freshers at TinkerMinds as part of AZ-204 capstone training.

---

## License

_TBD_
