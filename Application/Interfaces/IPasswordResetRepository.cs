using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPasswordResetRepository
    {
        Task<PasswordResetRequest> CreateAsync(PasswordResetRequest request);
        Task<PasswordResetRequest?> GetByTokenAsync(string token);
        Task UpdateAsync(PasswordResetRequest request);
    }
}
