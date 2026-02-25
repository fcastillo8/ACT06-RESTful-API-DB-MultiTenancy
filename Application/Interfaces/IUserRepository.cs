using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username, string tenantId);
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<IEnumerable<User>> GetAllByTenantAsync(string tenantId);
    }
}
