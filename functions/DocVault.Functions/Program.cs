using Azure.Storage.Blobs;
using DocVault.Functions.Services;
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

// Thumbnail generation service (SkiaSharp + PdfPig)
builder.Services.AddSingleton<IThumbnailService, ThumbnailService>();

builder.Build().Run();
