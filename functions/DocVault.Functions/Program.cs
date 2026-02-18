using Azure.Storage.Blobs;
using DocVault.Functions.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton(sp => new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage")))
    .AddSingleton(sp => new CosmosClient(Environment.GetEnvironmentVariable("CosmosDb:ConnectionString")))
    .AddScoped<IImageProcessingService, ImageProcessingService>()
    .AddScoped<ITextExtractionService, TextExtractionService>()
    .AddScoped<ICosmosMetadataService, CosmosMetadataService>();

builder.Build().Run();
