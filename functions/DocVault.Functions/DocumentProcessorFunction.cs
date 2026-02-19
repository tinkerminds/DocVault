using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocVault.Functions;

public class DocumentProcessorFunction
{
    private readonly ILogger _logger;

    public DocumentProcessorFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DocumentProcessorFunction>();
    }

    [Function("DocumentProcessorFunction")]
    public void Run(
        [ServiceBusTrigger("document-processing", Connection = "ServiceBusConnection")]
        string documentId)
    {
        _logger.LogInformation($"Processing document: {documentId}");
        _logger.LogInformation("Message consumed successfully.");
    }
}
