# .NET API Scaffolding Plan

> **Structure only — no endpoints or business logic implemented.**

---

## Initialization

```bash
# From the repo root
cd backend

# Create .NET 8 Web API with controllers
dotnet new webapi -n DocVault.Api --use-controllers --framework net8.0
```

### Key flags

| Flag                 | Reason                                              |
| -------------------- | --------------------------------------------------- |
| `--use-controllers`  | Use controller-based routing (not minimal APIs)     |
| `--framework net8.0` | Target .NET 8 LTS                                   |
| `-n DocVault.Api`    | Project name follows `<Product>.<Layer>` convention |

---

## Folder Structure

After scaffolding, create the following folders inside the project:

```
backend/DocVault.Api/
├── Controllers/           # API controllers
│   ├── DocumentsController.cs   # Upload, list, get, delete, search
│   └── HealthController.cs      # Health check for App Insights availability test
├── Services/              # Business logic services
│   ├── IBlobStorageService.cs    # Blob upload, SAS token generation
│   ├── BlobStorageService.cs
│   ├── ICosmosDbService.cs       # CRUD against documents container
│   ├── CosmosDbService.cs
│   └── IEventPublisher.cs        # Event Grid publishing (Day 3)
├── Models/                # DTOs, request/response models
│   ├── DocumentMetadata.cs       # Matches Cosmos DB schema
│   ├── UploadRequest.cs
│   ├── DocumentResponse.cs       # Includes SAS download URL
│   └── SearchResult.cs
├── Data/                  # Cosmos DB context, repository pattern
│   └── CosmosDbContext.cs
├── Config/                # Configuration classes, options pattern
│   ├── AzureStorageOptions.cs
│   ├── CosmosDbOptions.cs
│   └── EntraIdOptions.cs
├── Middleware/             # Custom middleware
│   └── TelemetryMiddleware.cs    # Custom metric tracking (Day 3)
├── Program.cs             # Entry point (generated)
├── appsettings.json       # App configuration (generated)
└── DocVault.Api.csproj    # Project file (generated)
```

---

## API Endpoints

| Method | Route                      | Description                                       | Day   |
| ------ | -------------------------- | ------------------------------------------------- | ----- |
| POST   | `/api/documents`           | Upload file (multipart/form-data) → Blob + Cosmos | Day 1 |
| GET    | `/api/documents`           | List current user's documents from Cosmos DB      | Day 1 |
| GET    | `/api/documents/{id}`      | Get single document metadata + SAS download URL   | Day 1 |
| DELETE | `/api/documents/{id}`      | Soft-delete a document                            | Day 1 |
| GET    | `/api/documents/search?q=` | Full-text search over excerpts                    | Day 2 |
| GET    | `/api/health`              | Health check (used by App Insights availability)  | Day 3 |

---

## NuGet Packages

### Day 1 — Storage & Data

```bash
cd backend/DocVault.Api

dotnet add package Microsoft.Azure.Cosmos
dotnet add package Azure.Storage.Blobs
dotnet add package Swashbuckle.AspNetCore
```

### Day 2 — Security & Identity

```bash
dotnet add package Microsoft.Identity.Web               # JWT bearer auth with Entra ID
dotnet add package Azure.Identity                       # Managed Identity / DefaultAzureCredential
dotnet add package Azure.Security.KeyVault.Secrets      # Key Vault secret retrieval
```

### Day 3 — Observability & Messaging

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore   # App Insights telemetry
dotnet add package Azure.Messaging.EventGrid                  # Event Grid publishing
dotnet add package Azure.Messaging.ServiceBus                 # Service Bus queue messaging
```

| Package                                    | Day   | Purpose                                      |
| ------------------------------------------ | ----- | -------------------------------------------- |
| `Microsoft.Azure.Cosmos`                   | Day 1 | Cosmos DB SDK                                |
| `Azure.Storage.Blobs`                      | Day 1 | Azure Blob Storage SDK                       |
| `Swashbuckle.AspNetCore`                   | Day 1 | Swagger/OpenAPI (included in template)       |
| `Microsoft.Identity.Web`                   | Day 2 | Entra ID JWT bearer authentication           |
| `Azure.Identity`                           | Day 2 | DefaultAzureCredential for Managed Identity  |
| `Azure.Security.KeyVault.Secrets`          | Day 2 | Read secrets from Key Vault                  |
| `Microsoft.ApplicationInsights.AspNetCore` | Day 3 | Server-side telemetry + custom metrics       |
| `Azure.Messaging.EventGrid`                | Day 3 | Publish DocumentUploaded events              |
| `Azure.Messaging.ServiceBus`               | Day 3 | Queue messages for heavy document processing |

---

## Middleware Pipeline (Program.cs)

The middleware should be registered in this order:

```
1. Application Insights telemetry         (Day 3)
2. CORS policy                            (Day 1)
3. Authentication (Entra ID JWT bearer)   (Day 2)
4. Authorization                          (Day 2)
5. Swagger (dev only)                     (Day 1)
6. Controller mapping                     (Day 1)
```

---

## Day-by-Day Implementation Plan

| Day   | What to Implement                                                                       |
| ----- | --------------------------------------------------------------------------------------- |
| Day 1 | Scaffold project, add Cosmos + Blob packages, build POST/GET/DELETE endpoints, CORS     |
| Day 2 | Add Entra ID auth middleware, Key Vault integration, search endpoint, Managed Identity  |
| Day 3 | Event Grid publishing on upload, Service Bus integration, App Insights, health endpoint |
| Day 4 | Dockerfile, containerize, or configure deployment slots                                 |
