# Azure Infrastructure Setup Script Plan

> **Step-by-step Azure CLI plan — to be converted into `infra/setup.sh` later.**
> **No secrets or keys are included.**

---

## Naming Convention

All resources follow the pattern:

```
docvault-<resource>-<env>
```

| Resource            | Example Name                                          |
| ------------------- | ----------------------------------------------------- |
| Resource Group      | `docvault-rg-dev`                                     |
| Storage Account     | `docvaultstoragedev` (no hyphens — Azure requirement) |
| Blob Container      | `uploads`                                             |
| Cosmos DB Account   | `docvault-cosmos-dev`                                 |
| Cosmos DB Database  | `docvault-db`                                         |
| Cosmos DB Container | `documents`                                           |

> **Note:** Storage account names must be 3-24 characters, lowercase alphanumeric only.

---

## Variables

```bash
RESOURCE_GROUP="docvault-rg-dev"
LOCATION="eastus"
STORAGE_ACCOUNT="docvaultstoragedev"
BLOB_CONTAINER="uploads"
COSMOS_ACCOUNT="docvault-cosmos-dev"
COSMOS_DATABASE="docvault-db"
COSMOS_CONTAINER="documents"
PARTITION_KEY="/userId"
```

---

## Step-by-Step Provisioning

### Step 1 — Create Resource Group

```bash
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

All DocVault resources live in a single resource group for easy management and teardown.

---

### Step 2 — Create Storage Account

```bash
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS \
  --kind StorageV2 \
  --access-tier Hot
```

| Setting        | Reason                                           |
| -------------- | ------------------------------------------------ |
| `Standard_LRS` | Locally redundant — sufficient for dev           |
| `StorageV2`    | General-purpose v2 — supports Blob, Queue, Table |
| `Hot`          | Frequently accessed documents                    |

---

### Step 3 — Create Blob Container

```bash
az storage container create \
  --name $BLOB_CONTAINER \
  --account-name $STORAGE_ACCOUNT \
  --public-access off
```

- Container name: `uploads`
- Public access is **off** — all access goes through the API

---

### Step 4 — Create Cosmos DB Account

```bash
az cosmosdb create \
  --name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --locations regionName=$LOCATION failoverPriority=0 \
  --default-consistency-level Session \
  --kind GlobalDocumentDB
```

| Setting               | Reason                                         |
| --------------------- | ---------------------------------------------- |
| `Session` consistency | Balanced read consistency for user-scoped data |
| `GlobalDocumentDB`    | SQL (Core) API                                 |

---

### Step 5 — Create Cosmos DB Database

```bash
az cosmosdb sql database create \
  --account-name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --name $COSMOS_DATABASE
```

---

### Step 6 — Create Cosmos DB Container

```bash
az cosmosdb sql container create \
  --account-name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --database-name $COSMOS_DATABASE \
  --name $COSMOS_CONTAINER \
  --partition-key-path $PARTITION_KEY \
  --throughput 400
```

| Setting                  | Reason                                             |
| ------------------------ | -------------------------------------------------- |
| Partition key: `/userId` | Queries are scoped per user — optimal partitioning |
| Throughput: `400 RU/s`   | Minimum manual throughput for dev                  |

---

## Verification

After running all steps, verify with:

```bash
# List resources in the group
az resource list --resource-group $RESOURCE_GROUP --output table

# Check Cosmos container
az cosmosdb sql container show \
  --account-name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --database-name $COSMOS_DATABASE \
  --name $COSMOS_CONTAINER \
  --query "{name:name, partitionKey:resource.partitionKey}"
```

---

## Teardown (Dev Only)

```bash
az group delete --name $RESOURCE_GROUP --yes --no-wait
```
