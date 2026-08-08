 
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(Employee user);
    }
}
