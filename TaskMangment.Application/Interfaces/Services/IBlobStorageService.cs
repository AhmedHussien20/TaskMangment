using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync( Stream stream,  string fileName, string contentType, string folder);
        Task DeleteAsync(string blobUrl);
        string WithSas(string urlWithoutSas);
    }

}
