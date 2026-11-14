using System.Security.Cryptography;
using System.Text;
using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Application.Mappers;
using AlertaComApi.Domain.Common;
using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlertaComApi.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AuthService> _logger;
        private readonly IHateoasService? _hateoasService;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<AuthService> logger,
            IHateoasService? hateoasService = null)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
            _hateoasService = hateoasService;
        }

        public async Task<OperationResult<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Tentativa de login para o usuário: {Username}", request.Username);

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Falha de autenticação para o usuário: {Username}", request.Username);
                return OperationResult<LoginResponseDto>.Fail("Credenciais inválidas. Verifique seu usuário e senha.");
            }

            var token = _jwtTokenGenerator.GenerateToken(user, out var expiresAt);

            var response = new LoginResponseDto(
                token,
                user.Username,
                user.Nome,
                user.Email,
                user.Role,
                expiresAt);

            _logger.LogInformation("Usuário {Username} autenticado com sucesso. Role: {Role}", user.Username, user.Role);
            return OperationResult<LoginResponseDto>.Ok(response, "Login realizado com sucesso.");
        }

        public async Task<OperationResult<UserDto>> RegisterAsync(RegisterRequestDto request)
        {
            _logger.LogInformation("Registrando novo cidadão: {Username} ({Email})", request.Username, request.Email);

            var exists = await _userRepository.ExistsByUsernameOrEmailAsync(request.Username, request.Email);
            if (exists)
            {
                return OperationResult<UserDto>.Fail("Nome de usuário ou e-mail já cadastrado na plataforma.");
            }

            var passwordHash = HashPassword(request.Password);
            var newUser = new User(
                request.Username,
                passwordHash,
                request.Nome,
                request.Email,
                request.Cpf,
                request.Bairro,
                string.IsNullOrWhiteSpace(request.Role) ? "Cidadao" : request.Role);

            newUser.Telefone = request.Telefone ?? string.Empty;

            var createdUser = await _userRepository.AddAsync(newUser);
            _logger.LogInformation("Cidadão {Username} registrado com sucesso (ID: {Id})", createdUser.Username, createdUser.Id);

            var dto = UserMapper.ToDto(createdUser);
            if (_hateoasService != null) dto = _hateoasService.BuildUserLinks(dto);
            return OperationResult<UserDto>.Ok(dto, "Cidadão cadastrado com sucesso.");
        }

        public async Task<OperationResult<UserDto>> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return OperationResult<UserDto>.Fail("Usuário não encontrado.");
            }

            var dto = UserMapper.ToDto(user);
            if (_hateoasService != null) dto = _hateoasService.BuildUserLinks(dto);
            return OperationResult<UserDto>.Ok(dto);
        }

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return string.Equals(hash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
