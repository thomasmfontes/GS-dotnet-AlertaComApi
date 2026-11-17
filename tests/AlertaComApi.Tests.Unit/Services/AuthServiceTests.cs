using System.Security.Cryptography;
using System.Text;
using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Application.Services;
using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AlertaComApi.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IJwtTokenGenerator> _jwtGenMock = new();
        private readonly Mock<ILogger<AuthService>> _loggerMock = new();
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _service = new AuthService(_userRepoMock.Object, _jwtGenMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task LoginAsync_DeveRetornarSucesso_QuandoCredenciaisSaoValidas()
        {
            // Arrange
            var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("123456")));
            var user = new User("thomas", passwordHash, "Thomas Fontes", "thomas@fiap.com.br", "12345678900", "Vila Prudente");

            _userRepoMock.Setup(r => r.GetByUsernameAsync("thomas"))
                .ReturnsAsync(user);

            var expiresAt = DateTime.UtcNow.AddHours(24);
            _jwtGenMock.Setup(j => j.GenerateToken(user, out expiresAt))
                .Returns("token-jwt-fake");

            // Act
            var result = await _service.LoginAsync(new LoginRequestDto("thomas", "123456"));

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Token.Should().Be("token-jwt-fake");
            result.Data.Username.Should().Be("thomas");
        }

        [Fact]
        public async Task LoginAsync_DeveFalhar_QuandoUsuarioNaoExiste()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetByUsernameAsync("inexistente"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _service.LoginAsync(new LoginRequestDto("inexistente", "qualquer"));

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Credenciais inválidas"));
        }
    }
}
