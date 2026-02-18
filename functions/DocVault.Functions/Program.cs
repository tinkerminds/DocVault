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
    .ConfigureFunctionsApplicationInsights();

// Azure Blob Storage — used to upload thumbnails to 'thumbnails' container
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var connectionString = builder.Configuration["BlobStorage"]
        ?? throw new InvalidOperationException("BlobStorage connection string is not configured.");
    return new BlobServiceClient(connectionString);
});

// Azure Cosmos DB — used to update document metadata (excerpt, thumbnailUrl, status)
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var connectionString = builder.Configuration["CosmosDb"]
        ?? throw new InvalidOperationException("CosmosDb connection string is not configured.");
    return new CosmosClient(connectionString, new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    });
});

// Services
builder.Services.AddSingleton<IThumbnailService, ThumbnailService>();
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

builder.Build().Run();
