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

---

# Day 2 — Security, Identity & Serverless

**Date**: February 18, 2026
**Team Member**: Ashish Tikhile
**Task**: Key Vault Setup + Managed Identity
**Branch**: `feature/keyvault`

---

## Task 1: Create Key Vault (Azure Portal)

### Steps

1. Go to **portal.azure.com** → search **"Key Vault"** in top search bar
2. Click **"Create"**
3. Fill in:
   - **Resource Group**: `docvault-rg`
   - **Key vault name**: `docvault-kv-dev`
   - **Region**: Central India
4. Click **"Review + Create"** → **"Create"**

**What it does**: Creates a secure, centralized secret store in Azure. No more connection strings in code or config files.

---

## Task 2: Add Secrets to Key Vault (Azure Portal)

### Steps

1. Open your Key Vault → click **"Secrets"** in left menu
2. Click **"+ Generate/Import"**
3. Add **Cosmos DB** secret:
   - **Name**: `ConnectionStrings--CosmosDb`
   - **Value**: your Cosmos DB connection string
   - Click **"Create"**
4. Add **Blob Storage** secret:
   - **Name**: `ConnectionStrings--BlobStorage`
   - **Value**: your Blob Storage connection string
   - Click **"Create"**

> **Why double dash `--`?** Azure Key Vault uses `--` instead of `:` to represent nested config keys. So `ConnectionStrings--CosmosDb` maps to `ConnectionStrings:CosmosDb` in .NET config.

**What it does**: Moves all sensitive connection strings out of `appsettings.json` into Key Vault. Secrets are encrypted at rest and access-controlled.

---

## Task 3: Enable System-Assigned Managed Identity on App Service

### Steps

1. Go to **App Services** → open your app
2. In left menu → scroll down → click **"Identity"**
3. Under **"System assigned"** tab → toggle **Status to "On"**
4. Click **"Save"** → confirm **"Yes"**
5. Copy the **Object (principal) ID** that appears — needed for next step

**What it does**: Gives your App Service its own identity in Azure Active Directory — like a user account for your app. It can now authenticate to other Azure services without passwords.

---

## Task 4: Grant App Service Access to Key Vault

### Steps

1. Go to your **Key Vault** → click **"Access policies"** in left menu
2. Click **"+ Create"**
3. Under **Permissions**:
   - Secret permissions → check **Get** and **List**
   - Click **Next**
4. Under **Principal** → paste the **Object ID** copied above → select your App Service → click **Next**
5. Click **"Create"**

**What it does**: Tells Key Vault "trust this App Service — allow it to read secrets." The App Service can now fetch connection strings automatically at runtime using its Managed Identity — no passwords needed anywhere.

---

## How It All Works Together

```
App Service (Managed Identity)
        ↓  authenticates automatically
   Azure Key Vault
        ↓  returns secrets securely
   CosmosDb connection string
   BlobStorage connection string
        ↓
   .NET API uses them at runtime
```

**No passwords in code. No secrets in Git. Fully secure.**

---

## Resources Created

| Resource Type   | Name                  | Location      | Purpose                        |
| --------------- | --------------------- | ------------- | ------------------------------ |
| Key Vault       | `docvault-kv-dev`   | Central India | Secure secret storage          |
| Secret          | `ConnectionStrings--CosmosDb`  | -  | Cosmos DB connection string    |
| Secret          | `ConnectionStrings--BlobStorage` | -| Blob Storage connection string |
| Managed Identity| System-assigned on App Service | - | Passwordless auth to Key Vault |

---

## Security Measures

✅ **No secrets in code** — all connection strings in Key Vault
✅ **No secrets in Git** — `local.settings.json` is gitignored
✅ **Managed Identity** — App Service authenticates without passwords
✅ **Least privilege** — only `Get` and `List` permissions granted

---

## Completion Checklist

- [ ] Key Vault `docvault-kv-dev` created in Central India
- [ ] Cosmos DB connection string added as secret
- [ ] Blob Storage connection string added as secret
- [ ] System-Assigned Managed Identity enabled on App Service
- [ ] Key Vault access policy granted to App Service
- [ ] Verified App Service can read secrets from Key Vault
- [ ] Branch `feature/keyvault` pushed and PR opened
