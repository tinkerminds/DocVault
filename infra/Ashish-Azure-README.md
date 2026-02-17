# Day 1 - Azure Cloud Setup COMPLETED

**Date**: February 17, 2026  
**Team Member**: Ashish Tikhile  
**Task**: Blob Storage Infrastructure Setup  
**Status**: Azure Phase Complete

---

## What We Built Today

Created the foundational Azure infrastructure for DocVault's blob storage service:

- Resource Group in Central India
- Storage Account with LRS redundancy
- Private blob container for uploads
- Retrieved connection string for backend integration

---

## Commands Executed

### 1. Resource Group Creation

```bash
az group create --name docvault-rg --location centralindia
```

**What it does**: Creates a logical container in Azure to organize all project resources  
**Why Central India**: Lowest latency for India-based users  
**Internal**: Azure Resource Manager provisions namespace in specified region

---

### 2. Storage Account Creation

```bash
az storage account create \
  --name docvaultstoragedev \
  --resource-group docvault-rg \
  --location centralindia \
  --sku Standard_LRS \
  --kind StorageV2
```

**What it does**: Provisions cloud storage infrastructure  
**Key Parameters**:

- **Standard_LRS**: Locally redundant storage (3 copies in same datacenter)
- **StorageV2**: Latest generation (supports all blob tiers: Hot/Cool/Archive)
  **Internal Process**:

1. Azure allocates storage nodes
2. Generates unique endpoint: `https://docvaultstoragedev.blob.core.windows.net/`
3. Creates 3 synchronous replicas for data durability
4. Sets up access keys and SAS token infrastructure

---

### 3. Blob Container Creation

```bash
az storage container create \
  --name uploads \
  --account-name docvaultstoragedev \
  --auth-mode login
```

**What it does**: Creates a logical folder within storage for organizing blobs  
**Public access off**: Security best practice - all access via authenticated API only
**Planned Structure**:

```
uploads/
└── {userId}/
    └── {documentId}/
        └── {fileName}
Example:
uploads/user-42/doc-789/invoice.pdf
```

**Why this structure?**

- User isolation (partition by userId)
- Easy cleanup (delete user's folder)
- Predictable paths for retrieval

---

### 4. Connection String Retrieval

```bash
az storage account show-connection-string \
  --name docvaultstoragedev \
  --resource-group docvault-rg \
  --query connectionString \
  --output tsv
```

**What it returns**: Authentication credentials for programmatic access  
**Format**: `DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net`
**Security Note**: Stored locally only - never committed to Git

---

## Key Concepts Learned

### Resource Group

- **Analogy**: Like a project folder on your laptop
- **Purpose**: Logical grouping, lifecycle management, billing boundary
- **Benefit**: Can delete entire project with one command

### Storage Account

- **Analogy**: Your personal Google Drive instance
- **Globally Unique**: Name must be unique across ALL Azure customers worldwide
- **Endpoint**: Every storage account gets a unique URL
- **Redundancy**: LRS keeps 3 copies to prevent data loss

### Blob Container

- **Analogy**: Folders inside Google Drive
- **Types**: Block blobs (files), Append blobs (logs), Page blobs (VHDs)
- **Access Levels**: Private (default), Blob (public read), Container (public list+read)

### Connection String

- **Components**:
  - Protocol: HTTPS only (secure)
  - Account Name: `docvaultstoragedev`
  - Account Key: 512-bit encryption key
  - Endpoint Suffix: `core.windows.net` (Azure public cloud)

---

##  How It Works Internally

### When a file is uploaded to Azure Blob Storage:

1. **Client sends request** → Authenticated via connection string/SAS token
2. **Azure Front End** → Routes to storage partition based on blob path
3. **Primary Storage Node** → Writes blob + metadata
4. **Replication** → Synchronously copies to 2 other nodes (LRS)
5. **Acknowledgment** → Returns HTTP 201 Created with blob URL

### Blob Naming Strategy:

```
userId/documentId/filename
```

**Why?**

- **Partition by userId**: Scales horizontally (Azure auto-balances)
- **documentId subfolder**: Prevents filename collisions
- **Original filename preserved**: Better UX + debugging

---

##  Resources Created

| Resource Type     | Name                 | Location      | Purpose                        |
| ----------------- | -------------------- | ------------- | ------------------------------ |
| Resource Group    | `docvault-rg`        | Central India | Logical container              |
| Storage Account   | `docvaultstoragedev` | Central India | Blob storage service           |
| Blob Container    | `uploads`            | -             | Document upload destination    |
| Connection String | (secured locally)    | -             | API authentication credentials |

## **Total Cost** (dev tier): ~₹150-200/month for 10GB + 10K transactions

## Security Measures

**No public access** - All blobs require authentication  
 **HTTPS only** - Encrypted in transit  
 **Connection string secured** - Not in source control  
 **RBAC ready** - Can add Managed Identity later (Day 2)

---

## Reference Links

- [Azure Blob Storage Docs](https://learn.microsoft.com/azure/storage/blobs/)
- [Storage Account Overview](https://learn.microsoft.com/azure/storage/common/storage-account-overview)
- [Connection Strings](https://learn.microsoft.com/azure/storage/common/storage-configure-connection-string)
- [Blob Naming Best Practices](https://learn.microsoft.com/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata)

---

## Completion Checklist

- [x] Azure Portal login successful
- [x] Resource Group `docvault-rg` created in Central India
- [x] Storage Account `docvaultstoragedev` provisioned
- [x] Container `uploads` created with private access
- [x] Connection string retrieved and secured
- [x] Verified resources in Azure Portal
- [x] Documented today's work


