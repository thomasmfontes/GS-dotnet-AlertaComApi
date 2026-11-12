using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Domain.Common;

namespace AlertaComApi.Application.Interfaces
{
    public interface IAuthService
    {
        Task<OperationResult<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<OperationResult<UserDto>> RegisterAsync(RegisterRequestDto request);
        Task<OperationResult<UserDto>> GetProfileAsync(int userId);
    }
}
