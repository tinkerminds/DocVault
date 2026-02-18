using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DocVault.Api.Services;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Microsoft Entra ID (Azure AD) JWT Bearer authentication
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

// CORS — allow Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",   // Angular dev server
                "http://localhost:5173",   // Vite dev server (if used)
                "https://yellow-river-03038cf00.4.azurestaticapps.net"  // Azure Static Web App
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---------- Secure Configuration: Key Vault or local env fallback ----------
//
// Production: set KeyVaultUrl env var → secrets fetched via Managed Identity (DefaultAzureCredential)
// Local dev:  set ConnectionStrings:CosmosDb and ConnectionStrings:BlobStorage in user secrets or env vars
//             (never hardcode secrets in appsettings files)

var keyVaultUrl = builder.Configuration["KeyVaultUrl"]
    ?? Environment.GetEnvironmentVariable("KeyVaultUrl");

string? cosmosConnectionString = null;
string? blobConnectionString = null;

if (!string.IsNullOrWhiteSpace(keyVaultUrl))
{
    // Production path: retrieve secrets from Azure Key Vault using Managed Identity
    var credential = new DefaultAzureCredential();
    var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);

    // Register SecretClient for any downstream services that may need it
    builder.Services.AddSingleton(secretClient);

    cosmosConnectionString = (await secretClient.GetSecretAsync("CosmosConnectionString")).Value.Value;
    blobConnectionString   = (await secretClient.GetSecretAsync("BlobConnectionString")).Value.Value;
}
else
{
    // Local development path: use environment variables or user secrets
    cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb")
        ?? Environment.GetEnvironmentVariable("CosmosDb__ConnectionString");

    blobConnectionString = builder.Configuration.GetConnectionString("BlobStorage")
        ?? Environment.GetEnvironmentVariable("BlobStorage__ConnectionString");
}

if (string.IsNullOrWhiteSpace(cosmosConnectionString))
    throw new InvalidOperationException(
        "Cosmos DB connection string is not configured. " +
        "Set KeyVaultUrl (production) or ConnectionStrings:CosmosDb (local dev).");

if (string.IsNullOrWhiteSpace(blobConnectionString))
    throw new InvalidOperationException(
        "Blob Storage connection string is not configured. " +
        "Set KeyVaultUrl (production) or ConnectionStrings:BlobStorage (local dev).");

// ---------- Azure Cosmos DB ----------

builder.Services.AddSingleton<CosmosClient>(_ =>
    new CosmosClient(cosmosConnectionString, new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    }));

builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddScoped<IDocumentSearchService, DocumentSearchService>();

// ---------- Azure Blob Storage ----------

builder.Services.AddSingleton<BlobServiceClient>(_ =>
    new BlobServiceClient(blobConnectionString));

builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

// ---------- App Pipeline ----------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
