  
using TaskMangment.Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace TaskMangment.Application.Interfaces.Services
{
    public class AttachmentBlobMigrationJob : IAttachmentBlobMigrationJob
    {
        private readonly AppDbContext _db;
        private readonly IBlobStorageService _blob;
        private readonly IWebHostEnvironment _env;

        public AttachmentBlobMigrationJob(
            AppDbContext db,
            IBlobStorageService blob,
            IWebHostEnvironment env)
        {
            _db = db;
            _blob = blob;
            _env = env;
        }

        public async Task ExecuteAsync()
        {
            var attachments = await _db.Attachments
                .Where(a =>
                    !a.IsUploadedToBlob &&
                    !a.IsDeleted &&
                    a.FilePath != null)
                .Take(20)
                .ToListAsync();

            foreach (var att in attachments)
            {
                string fullPath = string.Empty;

                try
                {
                    var relativePath = att.FilePath
                        .Replace("/", Path.DirectorySeparatorChar.ToString())
                        .TrimStart(Path.DirectorySeparatorChar);

                    fullPath = Path.Combine(_env.WebRootPath, relativePath);

                    if (!File.Exists(fullPath))
                    {
                        att.BlobUploadError = $"File not found on disk: {fullPath}";
                        continue;
                    }

                    using (var fileStream = new FileStream(
                        fullPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))  
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        var blobUrl = await _blob.UploadAsync(
                            memoryStream,
                            att.FileName,
                            att.ContentType,
                            "attachments"
                        );

                        att.BlobUrl = blobUrl;
                        att.FilePath = blobUrl;
                        att.IsUploadedToBlob = true;
                        att.BlobUploadedAt = DateTime.UtcNow;
                        att.BlobUploadError = null;
                    }

                    DeleteFileWithRetry(fullPath);
                }
                catch (Exception ex)
                {
                    att.BlobUploadError = ex.Message;
                }
            }

            await _db.SaveChangesAsync();
        }
        private static void DeleteFileWithRetry( string path,int retries = 5,int delayMs = 700)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(delayMs);
                }
                catch (UnauthorizedAccessException)
                {
                    Thread.Sleep(delayMs);
                }
            }
            throw new IOException($"Failed to delete file after retries: {path}");
        }


    }

}
