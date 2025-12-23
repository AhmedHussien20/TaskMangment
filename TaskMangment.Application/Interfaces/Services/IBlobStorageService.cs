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
        Task<string> UploadAsync(IFormFile file, string folder);
        Task DeleteAsync(string blobUrl);
    }

}
