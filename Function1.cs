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
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Blob name is null or empty.");
                return;
            }

            try
            {
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();

                var containerClient = _blobServiceClient.GetBlobContainerClient("rawcontracts");
                var blobClient = containerClient.GetBlobClient(name);

                var properties = await blobClient.GetPropertiesAsync();

                _logger.LogInformation(
                    "Processed blob {BlobName}: Size={Size} bytes, URI={BlobUri}",
                    name, properties.Value.ContentLength, blobClient.Uri);

                await using var sender = _serviceBusClient.CreateSender("messageFromFile");
                var message = new ServiceBusMessage(new BinaryData(new
                {
                    FileName = name,
                    Uri = blobClient.Uri.ToString(),
                    Size = properties.Value.ContentLength
                }));

                await sender.SendMessageAsync(message);
                _logger.LogInformation("Message sent to Service Bus for blob {BlobName}", name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing blob {BlobName}", name);
                throw;
            }
        }
    }
}
