#  DocVault – Cosmos DB Metadata Infrastructure

##  Overview

This module implements the **Cosmos DB metadata infrastructure** for the DocVault application.

It provides a fully functional repository layer for managing document metadata stored in **Azure Cosmos DB**, while maintaining a clean separation of concerns by keeping Blob Storage operations outside the repository layer.

The implementation is and successfully verified.

---

# 🏗 Architecture Summary

## Storage Design

- **Metadata** → Stored in Azure Cosmos DB  
- **Actual Files** → Stored in Azure Blob Storage  
- **blobUrl** → Stored as a string reference in Cosmos DB  

- Clean separation of concerns
- Scalable storage architecture
- Cost-efficient metadata queries

---

## Partition Key Strategy

**Partition Key:** `/userId`

- Enables efficient single-partition queries
- Reduces Request Unit (RU) consumption
- Supports horizontal scaling as users grow
- Optimized for user-scoped document operations

### Trade-offs

- Cross-user queries require cross-partition queries (not needed for this use case)
- Maximum 20GB per partition (acceptable for metadata)

---

#  Document Metadata Model

**File:** `DocumentMetadata.cs`

The model contains 11 required fields:

| Field | Type | Default | Purpose |
|-------|------|----------|----------|
| id | string | GUID | Unique document identifier |
| userId | string | - | Partition key (Entra ID object ID) |
| fileName | string | - | Original uploaded file name |
| blobUrl | string | - | Azure Blob Storage URL |
| contentType | string | - | MIME type (e.g., application/pdf) |
| sizeBytes | long | - | File size in bytes |
| uploadedAt | string | UTC Now | ISO 8601 timestamp |
| status | string | "pending" | Processing status |
| tags | List<string> | [] | User-assigned tags |
| excerpt | string | "" | Extracted preview text |
| thumbnailUrl | string | "" | Thumbnail image URL |

---

