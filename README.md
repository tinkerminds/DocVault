# DocVault

**Azure-based Document Management System**

DocVault is a cloud-native document management platform built on Azure services. It provides secure document upload, storage, metadata indexing, and retrieval through a modern web interface backed by a scalable API.

---

## Repository Structure

| Folder               | Purpose                                                            |
| -------------------- | ------------------------------------------------------------------ |
| `frontend/`          | Angular 17+ SPA — standalone components, routing, Angular Material |
| `backend/`           | .NET 8 Web API — controller-based REST API                         |
| `functions/`         | Azure Functions — placeholder for async processing (future)        |
| `infra/`             | Azure CLI scripts for infrastructure provisioning                  |
| `.github/workflows/` | GitHub Actions CI/CD pipeline definitions                          |
| `docs/`              | Architecture docs, ADRs, onboarding guides, and planning           |

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
```

---

## Branch Model

| Branch      | Purpose                 | Rules                                           |
| ----------- | ----------------------- | ----------------------------------------------- |
| `main`      | Production-ready code   | **Protected** — merge via PR only, CI must pass |
| `dev`       | Integration / QA branch | **Protected** — merge via PR only, CI must pass |
| `feature/*` | Task-based work         | Created from `dev`, merged back to `dev` via PR |

### Workflow

```
feature/add-upload-form  →  PR  →  dev  →  PR  →  main
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
feat(frontend): add document upload component
fix(api): correct blob URL generation
docs(infra): update Azure setup instructions
ci(workflow): add Angular build step
```

---

## License

_TBD_
