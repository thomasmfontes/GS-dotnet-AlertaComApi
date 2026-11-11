using AlertaComApi.Domain.Entities;

namespace AlertaComApi.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task<bool> ExistsByUsernameOrEmailAsync(string username, string email);
    }
}
