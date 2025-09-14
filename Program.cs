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
var storageConn = builder.Configuration["AzureWebJobsStorage"];

// Register BlobServiceClient
builder.Services.AddSingleton(new BlobServiceClient(storageConn));

// --- Resolve Service Bus connection string ---
var serviceBusConn = builder.Configuration["ServiceBusConnectionString"];

// Register ServiceBusClient
builder.Services.AddSingleton(new ServiceBusClient(serviceBusConn));

builder.Build().Run();
