using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocVault.Functions;

public class BlobTriggerFunction
{
    private readonly ILogger<BlobTriggerFunction> _logger;

    public BlobTriggerFunction(ILogger<BlobTriggerFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(BlobTriggerFunction))]
    public async Task Run(
        [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] Stream fileStream,
        string name,
        FunctionContext context)
    {
        _logger.LogInformation(
            "Blob trigger fired. File: {FileName}, Size: {FileSize} bytes",
            name,
            fileStream.Length);

        // TODO (Day 2): Generate thumbnail and save to 'thumbnails' container
        // TODO (Day 2): Extract text excerpt (first 500 chars)
        // TODO (Day 2): Update Cosmos DB metadata — set thumbnailUrl, excerpt, status = "processed"
        // TODO (Day 2): On failure, set status = "failed"

        await Task.CompletedTask;
    }
}
