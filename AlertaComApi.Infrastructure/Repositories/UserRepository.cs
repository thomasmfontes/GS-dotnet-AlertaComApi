using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Interfaces;
using AlertaComApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlertaComApi.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AlertaComContext _context;

        public UserRepository(AlertaComContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email)
        {
            return await _context.Users.AnyAsync(u =>
                u.Username.ToLower() == username.ToLower() ||
                u.Email.ToLower() == email.ToLower());
        }
    }
}
