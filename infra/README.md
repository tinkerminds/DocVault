# Infrastructure

Azure CLI scripts and templates for provisioning DocVault cloud resources.

## Resource Inventory

| Resource              | Name                    | Day     | AZ-204 Topic         |
| --------------------- | ----------------------- | ------- | -------------------- |
| Resource Group        | `docvault-rg-dev`       | Day 1   | —                    |
| Storage Account       | `docvaultstoragedev`    | Day 1   | Blob Storage         |
| Blob Containers       | `uploads`, `thumbnails` | Day 1–2 | Blob Storage         |
| Cosmos DB Account     | `docvault-cosmos-dev`   | Day 1   | Cosmos DB            |
| App Service Plan      | `docvault-plan-dev`     | Day 1   | App Service          |
| App Service (API)     | `docvault-api-dev`      | Day 1   | App Service          |
| Key Vault             | `docvault-kv-dev`       | Day 2   | Key Vault            |
| Function App          | `docvault-func-dev`     | Day 2   | Azure Functions      |
| Event Grid Topic      | `docvault-events-dev`   | Day 3   | Event Grid           |
| Service Bus Namespace | `docvault-sb-dev`       | Day 3   | Service Bus          |
| API Management        | `docvault-apim-dev`     | Day 3   | API Management       |
| Application Insights  | `docvault-insights-dev` | Day 3   | Application Insights |
| Container Registry    | `docvaultacrdev`        | Day 4   | Container Registry   |
| Container Apps Env    | `docvault-cae-dev`      | Day 4   | Container Apps       |

## Setup

See [`azure-setup-plan.md`](./azure-setup-plan.md) for the step-by-step provisioning plan with all Azure CLI commands.

## Teardown

```bash
az group delete --name docvault-rg-dev --yes --no-wait
```
