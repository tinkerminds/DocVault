using DocVault.Api.Services;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using Azure.Identity;
using Microsoft.Identity.Web;
using Azure.Messaging.EventGrid;

var builder = WebApplication.CreateBuilder(args);

// ---------- Key Vault Configuration (Managed Identity) ----------
// When deployed to Azure with a Key Vault name configured,
// secrets are loaded directly from Key Vault using Managed Identity.
var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrEmpty(keyVaultName))
{
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}

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

// Azure Cosmos DB
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("CosmosDb")
        ?? throw new InvalidOperationException("CosmosDb connection string is not configured.");

    return new CosmosClient(connectionString, new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    });
});
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

// Azure Blob Storage
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("BlobStorage")
        ?? throw new InvalidOperationException("BlobStorage connection string is not configured.");

    return new BlobServiceClient(connectionString);
});
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();


// Azure Event Grid
builder.Services.AddSingleton<EventGridPublisherClient>(sp =>
{
    var endpoint = builder.Configuration["EventGrid:TopicEndpoint"];
    var key = builder.Configuration["EventGrid:TopicKey"];
    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
        throw new InvalidOperationException("EventGrid TopicEndpoint or TopicKey is not configured.");
    return new EventGridPublisherClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));
});
builder.Services.AddSingleton<IEventGridService, EventGridService>();

// Application Insights Analytics Service
// Uses a named HttpClient to query the App Insights REST API.
// The App ID and API Key are read from config (Key Vault in production).
builder.Services.AddHttpClient("AppInsights", client =>
{
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

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
