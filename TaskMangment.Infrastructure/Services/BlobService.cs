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
        private readonly string _sas;
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
            _sas = sas;

            var service = new BlobServiceClient(new Uri(accountUrl + sas));
            _container = service.GetBlobContainerClient(container);
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder)
        {
            var blobName = $"{folder}/{Guid.NewGuid()}_{fileName}";
            var blob = _container.GetBlobClient(blobName);
            Console.WriteLine(blobName);
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            });
            Console.WriteLine(blob.Uri.GetLeftPart(UriPartial.Path));

            return blob.Uri.GetLeftPart(UriPartial.Path);
        }


        public async Task DeleteAsync(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
                return;

            var uri = new Uri(blobUrl);
            var path = uri.AbsolutePath.TrimStart('/');

            const string containerPrefix = "attachments/";
            if (path.StartsWith(containerPrefix, StringComparison.OrdinalIgnoreCase))
                path = path.Substring(containerPrefix.Length);

            await _container.DeleteBlobIfExistsAsync(path);
        }

        public string WithSas(string urlWithoutSas)
    => string.IsNullOrWhiteSpace(urlWithoutSas) ? urlWithoutSas : urlWithoutSas + _sas;

      

    }
}
