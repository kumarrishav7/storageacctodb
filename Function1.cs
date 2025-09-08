using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobStorageToDB
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly string _connectionString;

        // Inject connection string resolved in Program.cs
        public Function1(ILogger<Function1> logger, string connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;
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
            _logger.LogInformation("Blob Size: {Size} bytes", stream.Length);

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("samples-workitems");
            var blobClient = containerClient.GetBlobClient(name);

            _logger.LogInformation("Blob URI: {BlobUri}", blobClient.Uri);
        }
    }
}
