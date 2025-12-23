using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration; 
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration config)
        {
            var accountUrl = config["Blob:AccountUrl"];
            var sas = config["Blob:SasToken"];
            var container = config["Blob:Container"];

            if (string.IsNullOrWhiteSpace(accountUrl))
                throw new Exception("Blob:AccountUrl is missing");

            if (string.IsNullOrWhiteSpace(sas))
                throw new Exception("Blob:SasToken is missing");

            if (!sas.StartsWith("?"))
                sas = "?" + sas;

            if (string.IsNullOrWhiteSpace(container))
                throw new Exception("Blob:Container is missing");

            var service = new BlobServiceClient(new Uri(accountUrl + sas));
            _container = service.GetBlobContainerClient(container);
        }

        public async Task<string> UploadAsync(Stream stream,string fileName,string contentType,string folder)
        {
            var blobName = $"{folder}/{Guid.NewGuid()}_{fileName}";
            var blob = _container.GetBlobClient(blobName);

            await blob.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = contentType
                });

            return blob.Uri.ToString();
        }


        public async Task DeleteAsync(string blobUrl)
        {
            var blobName = new Uri(blobUrl).AbsolutePath
                .Split("/attachments/")[1];

            await _container.DeleteBlobIfExistsAsync(blobName);
        }
    }
}
