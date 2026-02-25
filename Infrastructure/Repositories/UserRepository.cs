using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username, string tenantId)
        {
            // IgnoreQueryFilters to search across specific tenant
            return await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == username && u.TenantId == tenantId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllByTenantAsync(string tenantId)
        {
            // Uses the global query filter
            return await _context.Users.ToListAsync();
        }
    }
}
