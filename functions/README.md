# Azure Functions — Document Processing

## Overview

Azure Functions provide event-driven, serverless processing of uploaded documents. They run automatically when documents are uploaded, handling thumbnail generation and text extraction without blocking the main API.

## Functions

### Function 1 — Blob Trigger (Day 2)

| Property    | Value                                                             |
| ----------- | ----------------------------------------------------------------- |
| **Trigger** | Blob trigger — fires when a new file lands in `uploads` container |
| **Runtime** | .NET 8 (isolated worker)                                          |
| **Input**   | The uploaded blob                                                 |
| **Output**  | Thumbnail in `thumbnails` container + updated Cosmos DB metadata  |

**Logic:**

1. Receive the new blob from the `uploads` container
2. Generate a thumbnail (for images and PDFs)
3. Save the thumbnail to the `thumbnails` Blob container
4. Extract text content from the document (first 500 characters)
5. Update the Cosmos DB metadata item: set `thumbnailUrl`, `excerpt`, and `status` → `processed`
6. If processing fails, set `status` → `failed`

---

### Function 2 — Service Bus Trigger (Day 3)

| Property    | Value                                                                 |
| ----------- | --------------------------------------------------------------------- |
| **Trigger** | Service Bus queue trigger — consumes from `document-processing` queue |
| **Runtime** | .NET 8 (isolated worker)                                              |
| **Input**   | Queue message with document ID and processing instructions            |
| **Output**  | Updated Cosmos DB metadata                                            |

**Logic:**

1. Receive a message from the `document-processing` Service Bus queue
2. Perform heavy processing tasks (e.g., OCR, full-text indexing)
3. Update the document metadata in Cosmos DB
4. Failed messages go to the dead-letter queue for retry or manual review

---

## Event Flow (Day 3 Architecture)

```
Upload API → Event Grid (DocumentUploaded event)
                ├── → Azure Function (Blob trigger) → Thumbnail + Excerpt
                └── → Notification endpoint

Upload API → Service Bus Queue (document-processing)
                └── → Azure Function (Service Bus trigger) → Heavy processing
```

> **Why Event Grid + Service Bus?** Event Grid for lightweight, fan-out notifications. Service Bus for reliable, ordered, heavy-processing jobs with dead-letter support.

---

## Security

- Functions use **Managed Identity** to access:
  - Azure Blob Storage (read uploads, write thumbnails)
  - Azure Cosmos DB (read/write metadata)
  - Azure Key Vault (retrieve connection strings)
- **No connection strings in code or config** — all secrets come from Key Vault via Managed Identity

---

## Project Structure (When Implemented)

```
functions/
├── DocVault.Functions/
│   ├── BlobTriggerFunction.cs       # Blob trigger — thumbnail + excerpt
│   ├── ServiceBusTriggerFunction.cs # Service Bus trigger — heavy processing
│   ├── host.json
│   ├── local.settings.json          # (gitignored — use Key Vault in prod)
│   └── DocVault.Functions.csproj
└── README.md
```

> This folder is a placeholder until Day 2. Implementation follows the plan above.
