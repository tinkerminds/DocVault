#!/bin/bash
# ============================================================
# DocVault — Key Vault + Managed Identity Setup
# Run this AFTER Day 1 resources (Storage, Cosmos, App Service)
# are already created.
# ============================================================

set -e

RESOURCE_GROUP="docvault-rg"
LOCATION="eastus"
KEY_VAULT="docvault-kv-dev"
STORAGE_ACCOUNT="docvaultstoragedev"
COSMOS_ACCOUNT="docvault-cosmos-dev"
APP_SERVICE_API="docvault-api-dev"
FUNCTION_APP="docvault-func-dev"

echo "=== Step 1: Create Key Vault ==="
az keyvault create \
  --name $KEY_VAULT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku standard

echo "=== Step 2: Store Cosmos DB connection string in Key Vault ==="
COSMOS_CONN=$(az cosmosdb keys list --name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP --type connection-strings \
  --query "connectionStrings[0].connectionString" -o tsv)

az keyvault secret set --vault-name $KEY_VAULT \
  --name "CosmosDbConnectionString" --value "$COSMOS_CONN"

echo "=== Step 3: Store Storage connection string in Key Vault ==="
STORAGE_CONN=$(az storage account show-connection-string \
  --name $STORAGE_ACCOUNT --resource-group $RESOURCE_GROUP \
  --query connectionString -o tsv)

az keyvault secret set --vault-name $KEY_VAULT \
  --name "StorageConnectionString" --value "$STORAGE_CONN"

echo "=== Step 4: Enable Managed Identity on App Service ==="
az webapp identity assign \
  --name $APP_SERVICE_API \
  --resource-group $RESOURCE_GROUP

# Grant App Service access to Key Vault secrets
API_PRINCIPAL_ID=$(az webapp identity show --name $APP_SERVICE_API \
  --resource-group $RESOURCE_GROUP --query principalId -o tsv)

az keyvault set-policy --name $KEY_VAULT \
  --object-id $API_PRINCIPAL_ID \
  --secret-permissions get list

echo "=== Step 5: Enable Managed Identity on Function App ==="
az functionapp identity assign \
  --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP

# Grant Function App access to Key Vault secrets
FUNC_PRINCIPAL_ID=$(az functionapp identity show --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP --query principalId -o tsv)

az keyvault set-policy --name $KEY_VAULT \
  --object-id $FUNC_PRINCIPAL_ID \
  --secret-permissions get list

echo "=== Step 6: Update App Service settings to use Key Vault references ==="
az webapp config appsettings set \
  --name $APP_SERVICE_API \
  --resource-group $RESOURCE_GROUP \
  --settings \
  ConnectionStrings__CosmosDb="@Microsoft.KeyVault(VaultName=$KEY_VAULT;SecretName=CosmosDbConnectionString)" \
  ConnectionStrings__BlobStorage="@Microsoft.KeyVault(VaultName=$KEY_VAULT;SecretName=StorageConnectionString)"

echo "=== Step 7: Update Function App settings to use Key Vault references ==="
az functionapp config appsettings set \
  --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP \
  --settings \
  CosmosDb="@Microsoft.KeyVault(VaultName=$KEY_VAULT;SecretName=CosmosDbConnectionString)" \
  BlobStorage="@Microsoft.KeyVault(VaultName=$KEY_VAULT;SecretName=StorageConnectionString)"

echo ""
echo "✅ Key Vault + Managed Identity setup complete!"
echo ""
echo "Verify with:"
echo "  az keyvault secret list --vault-name $KEY_VAULT -o table"
echo "  az webapp identity show --name $APP_SERVICE_API --resource-group $RESOURCE_GROUP"
echo "  az functionapp identity show --name $FUNCTION_APP --resource-group $RESOURCE_GROUP"
