 
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(Employee user);
    }
}
