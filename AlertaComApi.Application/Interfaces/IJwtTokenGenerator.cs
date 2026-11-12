using AlertaComApi.Domain.Entities;

namespace AlertaComApi.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user, out DateTime expiresAt);
    }
}
