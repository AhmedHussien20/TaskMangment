using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _container;
        private readonly string _sas;
        private readonly string _containerName;
        private readonly string _accountUrl;
        private readonly ILogger<BlobStorageService>? _logger;
        private readonly ICurrentUserService? _currentUser;

        public BlobStorageService(IConfiguration config, ICurrentUserService? currentUser = null, ILogger<BlobStorageService>? logger = null)
        {
            _logger = logger;
            _currentUser = currentUser;

            var accountUrl = config["Blob:AccountUrl"]
                ?? Environment.GetEnvironmentVariable("BLOB_ACCOUNT_URL");
            var sas = config["Blob:SasToken"]
                ?? Environment.GetEnvironmentVariable("BLOB_SAS_TOKEN");
            var container = config["Blob:Container"]
                ?? Environment.GetEnvironmentVariable("BLOB_CONTAINER")
                ?? "attachments";

            if (string.IsNullOrWhiteSpace(accountUrl))
                throw new Exception("Blob:AccountUrl is missing");

            if (string.IsNullOrWhiteSpace(sas))
                throw new Exception("Blob:SasToken is missing");

            if (!sas.StartsWith("?"))
                sas = "?" + sas;

            if (string.IsNullOrWhiteSpace(container))
                throw new Exception("Blob:Container is missing");

            _sas = sas;
            _containerName = container.Trim('/');
            _accountUrl = accountUrl.TrimEnd('/');

            var service = new BlobServiceClient(new Uri(_accountUrl + sas));
            _container = service.GetBlobContainerClient(_containerName);

            _logger?.LogInformation(
                "BlobStorage initialized. Account={AccountUrl}, Container={Container}",
                _accountUrl,
                _containerName);
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder)
        {
            // Azure returns InvalidUri for blob names containing U+FFFD or unpaired surrogates.
            var safeName = new string(fileName.Where(c => c != '\uFFFD' && !char.IsSurrogate(c)).ToArray());
            folder = PrefixTrainingFolder(folder);
            var blobName = $"{folder.Trim('/')}/{Guid.NewGuid()}_{safeName}";
            var blob = _container.GetBlobClient(blobName);
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            });

            return blob.Uri.GetLeftPart(UriPartial.Path);
        }

        public async Task DeleteAsync(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
                return;

            foreach (var blobName in GetBlobNameCandidates(blobUrl))
            {
                await _container.DeleteBlobIfExistsAsync(blobName);
            }
        }

        public string WithSas(string urlWithoutSas)
        {
            if (string.IsNullOrWhiteSpace(urlWithoutSas))
                return urlWithoutSas;

            if (urlWithoutSas.Contains('?', StringComparison.Ordinal))
                return urlWithoutSas;

            return urlWithoutSas + _sas;
        }

        public async Task<byte[]?> DownloadBytesAsync(string blobUrlOrPath)
        {
            if (string.IsNullOrWhiteSpace(blobUrlOrPath))
                return null;

            foreach (var blobName in GetBlobNameCandidates(blobUrlOrPath))
            {
                try
                {
                    var blob = _container.GetBlobClient(blobName);
                    if (!await blob.ExistsAsync())
                    {
                        _logger?.LogDebug("Blob not found for candidate name {BlobName}", blobName);
                        continue;
                    }

                    await using var stream = await blob.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    if (bytes.Length > 0)
                    {
                        _logger?.LogInformation(
                            "Downloaded blob {BlobName} ({Size} bytes)",
                            blobName,
                            bytes.Length);
                        return bytes;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed downloading blob candidate {BlobName}", blobName);
                }
            }

            // Last resort: HTTP GET with SAS (same path WhatsApp uses).
            try
            {
                var absolute = blobUrlOrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? WithSas(blobUrlOrPath)
                    : WithSas($"{_accountUrl}/{_containerName}/{blobUrlOrPath.TrimStart('/')}");

                using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
                using var response = await http.GetAsync(absolute);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes.Length > 0)
                    {
                        _logger?.LogInformation(
                            "Downloaded blob via HTTP SAS ({Size} bytes) from {Url}",
                            bytes.Length,
                            absolute.Split('?')[0]);
                        return bytes;
                    }
                }
                else
                {
                    _logger?.LogWarning(
                        "HTTP SAS download failed. Status={StatusCode}, Url={Url}",
                        (int)response.StatusCode,
                        absolute.Split('?')[0]);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "HTTP SAS download failed for {Path}", blobUrlOrPath);
            }

            return null;
        }

        private IEnumerable<string> GetBlobNameCandidates(string blobUrlOrPath)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in ExpandRawCandidates(blobUrlOrPath))
            {
                if (seen.Add(raw))
                    yield return raw;
            }
        }

        private IEnumerable<string> ExpandRawCandidates(string blobUrlOrPath)
        {
            var value = blobUrlOrPath.Trim();
            var q = value.IndexOf('?', StringComparison.Ordinal);
            if (q >= 0)
                value = value[..q];

            string path;
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                path = Uri.UnescapeDataString(uri.AbsolutePath).Trim('/');
            else
                path = value.Trim('/');

            yield return path;

            var prefix = _containerName + "/";
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var withoutContainer = path[prefix.Length..];
                yield return withoutContainer;

                // Handle nested folder named same as container: attachments/attachments/file
                if (withoutContainer.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    yield return withoutContainer[prefix.Length..];
            }
            else
            {
                // If stored as relative "folder/file", also try with container prefix stripped variants.
                yield return path;
            }

            var fileName = Path.GetFileName(path);
            if (!string.IsNullOrWhiteSpace(fileName) && fileName != path)
            {
                yield return fileName;
                yield return $"attachments/{fileName}";
            }
        }

        private string PrefixTrainingFolder(string folder)
        {
            var schema = _currentUser?.Schema;
            if (string.IsNullOrWhiteSpace(schema))
                schema = OperationalSchemaAmbient.Current;

            if (!string.Equals(schema, OperationalSchema.Training, StringComparison.OrdinalIgnoreCase))
                return folder;

            // Request users carry CompanyId on the JWT; Hangfire sets OperationalCompanyAmbient
            // for the trainee copy being processed so uploads never land under training/0.
            var companyId = _currentUser?.CompanyId
                ?? OperationalCompanyAmbient.Current;
            if (companyId is null or <= 0)
                throw new InvalidOperationException(
                    "Training blob uploads require a company id (JWT CompanyId or OperationalCompanyAmbient).");

            var trimmed = (folder ?? string.Empty).Trim('/');
            return string.IsNullOrEmpty(trimmed)
                ? $"training/{companyId}"
                : $"training/{companyId}/{trimmed}";
        }
    }
}
