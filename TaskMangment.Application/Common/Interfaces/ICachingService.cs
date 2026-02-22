using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Common.Interfaces
{
    public interface ICachingService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);

        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration);

    }

}
