using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace ApiExamenCubosJorgeRodriguez.Services
{
    public class BlobService
    {
        private BlobServiceClient _client;

        public BlobService(string connectionString)
        {
            _client = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadBlobAsync(string containerName, string fileName, Stream stream)
        {
            var containerClient = _client.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(stream, true);
            return blobClient.Uri.ToString();
        }

        // INVESTIGACIÓN: ¿Cómo devolver imagen de contenedor protegido?
        // RESPUESTA: Usando un SAS Token (Shared Access Signature) temporal.
        public string GetProtectedBlobUrl(string containerName, string fileName)
        {
            var blobClient = _client.GetBlobContainerClient(containerName).GetBlobClient(fileName);

            if (!blobClient.Exists()) return null;

            // Generamos una firma que dure, por ejemplo, 30 minutos
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        public string GetBlobUrl(string containerName, string fileName)
        {
            var blobClient = _client.GetBlobContainerClient(containerName).GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }
    }
}
