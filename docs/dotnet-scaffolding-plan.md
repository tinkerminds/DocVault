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
├── Controllers/           # API controllers (empty for now)
│   └── README.md
├── Services/              # Business logic services (empty for now)
│   └── README.md
├── Models/                # DTOs, request/response models (empty for now)
│   └── README.md
├── Data/                  # Cosmos DB context, repository pattern (empty for now)
│   └── README.md
├── Config/                # Configuration classes, options pattern (empty for now)
│   └── README.md
├── Program.cs             # Entry point (generated)
├── appsettings.json       # App configuration (generated)
└── DocVault.Api.csproj    # Project file (generated)
```

---

## NuGet Packages to Install (Day 1 prep)

These packages should be added so the project is ready for development:

```bash
cd backend/DocVault.Api

dotnet add package Microsoft.Azure.Cosmos
dotnet add package Azure.Storage.Blobs
dotnet add package Swashbuckle.AspNetCore
```

| Package                  | Purpose                                        |
| ------------------------ | ---------------------------------------------- |
| `Microsoft.Azure.Cosmos` | Cosmos DB SDK                                  |
| `Azure.Storage.Blobs`    | Azure Blob Storage SDK                         |
| `Swashbuckle.AspNetCore` | Swagger/OpenAPI (already included in template) |

---

## What's NOT Done on Day 1

- No controllers or endpoints
- No service implementations
- No Cosmos DB client setup in `Program.cs`
- No authentication middleware
- No CORS configuration
