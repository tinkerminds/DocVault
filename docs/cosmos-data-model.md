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
  "status": "uploaded"
}
```

---

## Field Definitions

| Field         | Type       | Required | Description                                                       |
| ------------- | ---------- | -------- | ----------------------------------------------------------------- |
| `id`          | `string`   | ✅       | Unique document identifier (GUID). Cosmos DB primary key.         |
| `userId`      | `string`   | ✅       | Owner's user ID. **Partition key** — all queries scoped per user. |
| `fileName`    | `string`   | ✅       | Original file name as uploaded by the user.                       |
| `blobUrl`     | `string`   | ✅       | Full URL to the file in Azure Blob Storage.                       |
| `contentType` | `string`   | ✅       | MIME type (e.g., `application/pdf`, `image/png`).                 |
| `sizeBytes`   | `number`   | ✅       | File size in bytes.                                               |
| `uploadedAt`  | `string`   | ✅       | ISO 8601 UTC timestamp of when the file was uploaded.             |
| `tags`        | `string[]` | ❌       | User-defined tags for categorization. Defaults to `[]`.           |
| `status`      | `string`   | ✅       | Document lifecycle status.                                        |

---

## Status Values

| Status       | Meaning                                             |
| ------------ | --------------------------------------------------- |
| `uploaded`   | File stored in Blob, metadata saved                 |
| `processing` | Async processing in progress (e.g., indexing, OCR)  |
| `ready`      | Processing complete, document fully available       |
| `failed`     | Processing failed — requires retry or manual review |

---

## Design Decisions

1. **Partition key `/userId`** — Most queries fetch "my documents", so partitioning by user ensures single-partition reads.
2. **`blobUrl` stored in metadata** — Avoids re-computing the URL; the API can return it directly.
3. **`tags` as string array** — Enables flexible tagging without a separate tags collection.
4. **`status` as string enum** — Simple state machine for document lifecycle tracking.
