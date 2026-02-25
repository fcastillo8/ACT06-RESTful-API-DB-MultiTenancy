using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetRequest> CreateAsync(PasswordResetRequest request)
        {
            _context.PasswordResetRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<PasswordResetRequest?> GetByTokenAsync(string token)
        {
            return await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.ResetToken == token && !r.IsUsed);
        }

        public async Task UpdateAsync(PasswordResetRequest request)
        {
            _context.PasswordResetRequests.Update(request);
            await _context.SaveChangesAsync();
        }
    }
}
