using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// --- Resolve Storage connection string ---
string storageConn;
var localStorageConn = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
if (!string.IsNullOrEmpty(localStorageConn))
{
    storageConn = localStorageConn;
}
else
{
    var keyVaultUrl = Environment.GetEnvironmentVariable("KEYVAULT_URL");
    if (string.IsNullOrEmpty(keyVaultUrl))
    {
        throw new InvalidOperationException("KEYVAULT_URL is not configured.");
    }

    var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    var secret = client.GetSecret("storageaccountconnstring").Value;
    storageConn = secret.Value;
}

// Register BlobServiceClient
builder.Services.AddSingleton(new BlobServiceClient(storageConn));

// --- Resolve Service Bus connection string ---
string serviceBusConn;
var localServiceBusConn = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
if (!string.IsNullOrEmpty(localServiceBusConn))
{
    serviceBusConn = localServiceBusConn;
}
else
{
    var keyVaultUrl = Environment.GetEnvironmentVariable("KEYVAULT_URL");
    if (string.IsNullOrEmpty(keyVaultUrl))
    {
        throw new InvalidOperationException("KEYVAULT_URL is not configured.");
    }

    var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    var secret = client.GetSecret("servicebusconnstring").Value;
    serviceBusConn = secret.Value;
}

// Register ServiceBusClient
builder.Services.AddSingleton(new ServiceBusClient(serviceBusConn));

builder.Build().Run();
