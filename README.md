# DocVault

**Secure Document Management Platform — AZ-204 Capstone Project**

DocVault is an internal document management platform where employees upload, search, and download files. Built with Angular + .NET Core, it wires together every major AZ-204 Azure service for genuine architectural reasons.

---

## Repository Structure

| Folder               | Purpose                                                            |
| -------------------- | ------------------------------------------------------------------ |
| `frontend/`          | Angular 17+ SPA — standalone components, routing, Angular Material |
| `backend/`           | .NET 8 Web API — controller-based REST API                         |
| `functions/`         | Azure Functions — Blob trigger + Service Bus trigger processing    |
| `infra/`             | Azure CLI scripts for infrastructure provisioning                  |
| `.github/workflows/` | GitHub Actions CI/CD pipeline definitions                          |
| `docs/`              | Architecture docs, data models, scaffolding plans, team rules      |

---

## Azure Service Map

| Azure Service        | Why It's Used                                                   | Day   |
| -------------------- | --------------------------------------------------------------- | ----- |
| App Service          | Host the .NET API (and optionally the Angular SPA)              | Day 1 |
| Blob Storage         | Store uploaded documents and generated thumbnails               | Day 1 |
| Cosmos DB            | Store document metadata, tags, audit logs (NoSQL)               | Day 1 |
| SAS Tokens           | Generate time-limited download links for documents              | Day 1 |
| Lifecycle Policy     | Auto-tier old documents to Cool (30d) / Archive (180d)          | Day 1 |
| Azure Functions      | Blob-triggered thumbnail generation + text extraction           | Day 2 |
| Microsoft Entra ID   | Authenticate users via MSAL; protect API with bearer tokens     | Day 2 |
| Key Vault            | Store Cosmos DB keys, Storage connection strings                | Day 2 |
| Managed Identity     | Let App Service + Functions access Key Vault without secrets    | Day 2 |
| Event Grid           | Publish DocumentUploaded event; decouple upload from processing | Day 3 |
| Service Bus          | Queue document-processing jobs for reliable background work     | Day 3 |
| API Management       | Front the API with rate-limiting, CORS, caching policies        | Day 3 |
| Application Insights | Telemetry, custom metrics, availability tests                   | Day 3 |
| Container Registry   | Store Docker images for the API                                 | Day 4 |
| Container Apps       | Deploy containerized API with scale-to-zero                     | Day 4 |

---

## API Endpoints

| Method | Route                      | Description                              |
| ------ | -------------------------- | ---------------------------------------- |
| POST   | `/api/documents`           | Upload file (multipart/form-data)        |
| GET    | `/api/documents`           | List current user's documents            |
| GET    | `/api/documents/{id}`      | Get document metadata + SAS download URL |
| DELETE | `/api/documents/{id}`      | Soft-delete a document                   |
| GET    | `/api/documents/search?q=` | Full-text search over excerpts           |
| GET    | `/api/health`              | Health check for availability tests      |

---

## Prerequisites

| Tool        | Version |
| ----------- | ------- |
| Node.js     | 18+     |
| Angular CLI | 17+     |
| .NET SDK    | 8.0+    |
| Azure CLI   | 2.50+   |
| Git         | 2.40+   |

---

## Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/<org>/DocVault.git
cd DocVault

# 2. Frontend
cd frontend
npm install
ng serve

# 3. Backend
cd ../backend/DocVault.Api
dotnet restore
dotnet run

# 4. Azure Resources (requires Azure CLI login)
cd ../../infra
# Follow azure-setup-plan.md
```

---

## Branch Model

| Branch      | Purpose                 | Rules                                           |
| ----------- | ----------------------- | ----------------------------------------------- |
| `main`      | Production-ready code   | **Protected** — merge via PR only, CI must pass |
| `dev`       | Integration / QA branch | **Protected** — merge via PR only, CI must pass |
| `feature/*` | Task-based work         | Created from `dev`, merged back to `dev` via PR |

```
feature/<task-name>  →  PR  →  dev  →  PR (end-of-day)  →  main  →  auto-deploy
```

---

## Commit Convention

Format: **`type(scope): description`**

| Type       | Use for                               |
| ---------- | ------------------------------------- |
| `feat`     | New feature                           |
| `fix`      | Bug fix                               |
| `docs`     | Documentation only                    |
| `chore`    | Maintenance, deps, config             |
| `ci`       | CI/CD pipeline changes                |
| `refactor` | Code restructure (no behavior change) |
| `test`     | Adding or updating tests              |

**Examples:**

```
feat(api): add upload endpoint with blob storage
fix(auth): correct MSAL redirect URI
chore(infra): add Key Vault resource to setup script
ci(deploy): add App Service deployment step
```

---

## 4-Day Sprint Overview

| Day   | Focus                     | Deliverable                                                  |
| ----- | ------------------------- | ------------------------------------------------------------ |
| Day 1 | Foundation & Storage      | Upload/download flow, Blob + Cosmos DB, CI/CD pipeline green |
| Day 2 | Security & Serverless     | Entra ID login, Key Vault, Azure Functions processing        |
| Day 3 | Events & Observability    | Event Grid, Service Bus, APIM, App Insights                  |
| Day 4 | Containers, Polish & Demo | Docker/Container Apps, dashboard, architecture diagram, demo |

---

## License

_TBD_
