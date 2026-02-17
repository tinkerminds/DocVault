# Backend — .NET 8 Web API

## Overview

The backend is a **.NET 8 Web API** with controller-based routing. It handles document uploads, metadata CRUD against Cosmos DB, SAS token generation for secure downloads, and publishes events for async processing.

## Tech Stack

| Layer         | Technology                         |
| ------------- | ---------------------------------- |
| Runtime       | .NET 8 LTS                         |
| Routing       | Controller-based (not minimal)     |
| Data          | Azure Cosmos DB (SQL API)          |
| Storage       | Azure Blob Storage                 |
| Auth          | Microsoft Entra ID (JWT bearer)    |
| Secrets       | Azure Key Vault + Managed Identity |
| Observability | Application Insights               |
| API Gateway   | Azure API Management               |

## API Endpoints

| Method | Route                      | Description                              |
| ------ | -------------------------- | ---------------------------------------- |
| POST   | `/api/documents`           | Upload file → Blob + Cosmos metadata     |
| GET    | `/api/documents`           | List current user's documents            |
| GET    | `/api/documents/{id}`      | Get document metadata + SAS download URL |
| DELETE | `/api/documents/{id}`      | Soft-delete a document                   |
| GET    | `/api/documents/search?q=` | Full-text search over excerpts           |
| GET    | `/api/health`              | Health check for availability tests      |

## Project Structure

```
backend/DocVault.Api/
├── Controllers/        # DocumentsController, HealthController
├── Services/           # BlobStorageService, CosmosDbService, EventPublisher
├── Models/             # DTOs, request/response models
├── Data/               # Cosmos DB context
├── Config/             # Options pattern classes
├── Middleware/          # Telemetry middleware
├── Program.cs
├── appsettings.json
└── DocVault.Api.csproj
```

## Quick Start

```bash
cd backend/DocVault.Api
dotnet restore
dotnet run
```

> See [`docs/dotnet-scaffolding-plan.md`](../docs/dotnet-scaffolding-plan.md) for full initialization and NuGet package details.
