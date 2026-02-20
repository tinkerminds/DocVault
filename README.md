# DocVault

**Secure Document Management Platform** — AZ-204 Capstone Project

DocVault is a document management app where employees can upload, search, and download files. Built with Angular for the frontend and .NET 8 for the backend, connected with Azure services to demonstrate real-world cloud architecture.

> Built by freshers at TinkerMinds as part of AZ-204 capstone training.

---

## 🔗 Live Links

| Resource                 | Link                                                                                                  |
| ------------------------ | ----------------------------------------------------------------------------------------------------- |
| **Working App**          | [yellow-river-03038cf00.4.azurestaticapps.net](https://yellow-river-03038cf00.4.azurestaticapps.net/) |
| **Backend API**          | [docvault-api-dev.azurewebsites.net](https://docvault-api-dev.azurewebsites.net/api/health)                      |
| **Architecture Diagram** | [View on Eraser](https://app.eraser.io/workspace/fnLL7kZHXVIuqJpjEpO4)                                |


## 🔐 Test Credentials

Use the following credentials to log in and explore the app:

| Field        | Value                         |
| ------------ | ----------------------------- |
| **Email**    | `ganeshjadhavar2003@gmail.com`|
| **Password** | `Laymarts@krishna916`         |

> [!NOTE]
> These are demo credentials for testing purposes only.

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

## Azure Services Used

| Service              | Purpose                                                              |
| -------------------- | -------------------------------------------------------------------- |
| App Service          | Host the .NET API                                                    |
| Static Web Apps      | Host the Angular SPA                                                 |
| Blob Storage         | Store uploaded documents + thumbnails                                |
| Cosmos DB            | Store document metadata, tags, audit logs                            |
| Azure Functions      | Blob-triggered thumbnail generation + text extraction                |
| Microsoft Entra ID   | User login via MSAL, API protected with JWT tokens                   |
| Key Vault            | Store all connection strings and secrets securely                    |
| Managed Identity     | Let App Service + Functions access Key Vault without secrets in code |
| Event Grid           | Publish DocumentUploaded event, decouple upload from processing      |
| Service Bus          | Queue processing jobs for reliable background work                   |
| API Management       | Rate-limiting, CORS, caching policies in front of the API            |
| Application Insights | Telemetry, custom metrics, availability tests                        |

---

## API Endpoints

| Method | Route                      | Description                              |
| ------ | -------------------------- | ---------------------------------------- |
| POST   | `/api/documents`           | Upload a file (multipart/form-data)      |
| GET    | `/api/documents`           | List current user's documents            |
| GET    | `/api/documents/{id}`      | Get document metadata + SAS download URL |
| DELETE | `/api/documents/{id}`      | Soft-delete a document                   |
| GET    | `/api/documents/search?q=` | Search documents by text content         |
| GET    | `/api/health`              | Health check                             |

---

## How to Run Locally

### Prerequisites

| Tool        | Version |
| ----------- | ------- |
| Node.js     | 20+     |
| Angular CLI | 19+     |
| .NET SDK    | 8.0+    |
| Azure CLI   | 2.50+   |

### Steps

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

## CI/CD Pipeline

Two GitHub Actions workflows are configured:

- **`ci.yml`** — Runs on every PR to `dev` and `main`. Builds and tests both frontend and backend. If CI fails, the PR cannot be merged.
- **`deploy.yml`** — Runs when code lands on `main`. Deploys the .NET API to Azure App Service and the Angular app to Azure Static Web Apps.

All credentials are stored as **GitHub Secrets** — nothing is hardcoded.

---

## License

_TBD_
