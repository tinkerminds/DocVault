# Cosmos DB Data Model Agreement

> **Schema planning only — no implementation.**

---

## Container: `documents`

| Property           | Partition Key |
| ------------------ | ------------- |
| Name               | `documents`   |
| Partition Key Path | `/userId`     |
| Database           | `docvault-db` |

---

## Document Metadata Schema

Each item in the `documents` container represents metadata for a single uploaded document.

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "user-abc-123",
  "fileName": "project-proposal.pdf",
  "blobUrl": "https://docvaultstoragedev.blob.core.windows.net/uploads/user-abc-123/550e8400.pdf",
  "contentType": "application/pdf",
  "sizeBytes": 245760,
  "uploadedAt": "2026-02-17T10:30:00Z",
  "tags": ["proposal", "Q1-2026", "engineering"],
  "excerpt": "This document outlines the Q1 2026 engineering project proposal for...",
  "thumbnailUrl": "https://docvaultstoragedev.blob.core.windows.net/thumbnails/user-abc-123/550e8400.png",
  "status": "processed"
}
```

---

## Field Definitions

| Field          | Type       | Required | Description                                                                      |
| -------------- | ---------- | -------- | -------------------------------------------------------------------------------- |
| `id`           | `string`   | ✅       | Unique document identifier (GUID). Cosmos DB primary key.                        |
| `userId`       | `string`   | ✅       | Entra ID object ID of the uploader. **Partition key** — queries scoped per user. |
| `fileName`     | `string`   | ✅       | Original file name as uploaded by the user.                                      |
| `blobUrl`      | `string`   | ✅       | Full URL to the file in Azure Blob Storage.                                      |
| `contentType`  | `string`   | ✅       | MIME type (e.g., `application/pdf`, `image/png`).                                |
| `sizeBytes`    | `number`   | ✅       | File size in bytes.                                                              |
| `uploadedAt`   | `string`   | ✅       | ISO 8601 UTC timestamp of when the file was uploaded.                            |
| `tags`         | `string[]` | ❌       | User-assigned tags for categorization. Defaults to `[]`.                         |
| `excerpt`      | `string`   | ❌       | Extracted text (first 500 chars). Populated by Azure Function after processing.  |
| `thumbnailUrl` | `string`   | ❌       | Thumbnail blob URL in the `thumbnails` container. Populated by Azure Function.   |
| `status`       | `string`   | ✅       | Document lifecycle status (see below).                                           |

---

## Status Values

| Status      | Meaning                                                   |
| ----------- | --------------------------------------------------------- |
| `pending`   | File stored in Blob, metadata saved — awaiting processing |
| `processed` | Azure Function completed thumbnail + text extraction      |
| `failed`    | Processing failed — requires retry or manual review       |

---

## API Endpoints Using This Model

| Method | Route                      | Cosmos DB Operation                      |
| ------ | -------------------------- | ---------------------------------------- |
| POST   | `/api/documents`           | Create metadata item after Blob upload   |
| GET    | `/api/documents`           | Query by `userId` (partition key)        |
| GET    | `/api/documents/{id}`      | Point read by `id` + `userId`            |
| DELETE | `/api/documents/{id}`      | Soft-delete (update `status` field)      |
| GET    | `/api/documents/search?q=` | Cross-partition query on `excerpt` field |

---

## Design Decisions

1. **Partition key `/userId`** — Most queries fetch "my documents", so partitioning by user ensures single-partition reads.
2. **`blobUrl` stored in metadata** — Avoids re-computing the URL; the API generates SAS tokens on-the-fly for download links (never exposing raw blob URLs to the client).
3. **`tags` as string array** — Enables flexible tagging without a separate tags collection.
4. **`status` as string enum** — Simple state machine: `pending` → `processed` | `failed`. Set to `pending` on upload, updated by Azure Function after processing.
5. **`excerpt` populated asynchronously** — Azure Function (Blob trigger or Service Bus trigger) extracts text and updates the Cosmos DB item. Enables full-text search via `GET /api/documents/search?q=`.
6. **`thumbnailUrl` populated asynchronously** — Azure Function generates a thumbnail for images/PDFs and stores it in a separate `thumbnails` Blob container. The URL is written back to the metadata item.
7. **SAS tokens for downloads** — The API never returns `blobUrl` directly to the client. Instead, `GET /api/documents/{id}` generates a time-limited SAS URL for secure download.
