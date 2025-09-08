using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var localConn = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
if (!string.IsNullOrEmpty(localConn))
{
    builder.Services.AddSingleton(localConn);
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

    builder.Services.AddSingleton(secret.Value);
}

builder.Build().Run();
