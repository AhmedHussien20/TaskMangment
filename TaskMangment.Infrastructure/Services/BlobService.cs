using Azure.Storage.Blobs;
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

            var service = new BlobServiceClient(new Uri(accountUrl + sas));
            _container = service.GetBlobContainerClient(container);
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            var blobName = $"{folder}/{Guid.NewGuid()}_{file.FileName}";
            var blob = _container.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);

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
