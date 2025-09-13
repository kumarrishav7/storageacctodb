using Azure.Storage.Blobs;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobStorageToDB
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ServiceBusClient _serviceBusClient;

        public Function1(
            ILogger<Function1> logger,
            BlobServiceClient blobServiceClient,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _serviceBusClient = serviceBusClient;
        }

        [Function(nameof(Function1))]
        public async Task Run(
            [BlobTrigger("rawcontracts/{name}", Connection = "AzureWebJobsStorage")] Stream stream,
            string name)
        {
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            _logger.LogInformation("Blob trigger function processed blob");
            _logger.LogInformation("Blob Name: {BlobName}", name);

            var containerClient = _blobServiceClient.GetBlobContainerClient("rawcontracts");
            var blobClient = containerClient.GetBlobClient(name);

            var properties = await blobClient.GetPropertiesAsync();
            _logger.LogInformation("Blob Size: {Size} bytes", properties.Value.ContentLength);
            _logger.LogInformation("Blob URI: {BlobUri}", blobClient.Uri);

            // --- Send a message to Service Bus ---
            var sender = _serviceBusClient.CreateSender("messageFromFile");

            var message = new ServiceBusMessage(new BinaryData(new
            {
                FileName = name,
                Uri = blobClient.Uri.ToString(),
                Size = properties.Value.ContentLength
            }));

            await sender.SendMessageAsync(message);
            _logger.LogInformation("Message sent to Service Bus for blob {BlobName}", name);
        }
    }
}
