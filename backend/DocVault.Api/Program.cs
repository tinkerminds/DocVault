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
                "http://localhost:5173"    // Vite dev server (if used)
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
