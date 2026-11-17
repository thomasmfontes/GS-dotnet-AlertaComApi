using System.Net;
using System.Net.Http.Json;
using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Tests.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace AlertaComApi.Tests.Integration.Endpoints
{
    public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_DeveRetornar200ETokenJwt_QuandoCredenciaisValidas()
        {
            // Arrange (usuário seed do DbInitializer)
            var request = new LoginRequestDto("cidadao", "123456");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            body.Should().NotBeNull();
            body!.Token.Should().NotBeNullOrWhiteSpace();
            body.Username.Should().Be("cidadao");
            body.Nome.Should().Be("Thomas Fontes");
        }

        [Fact]
        public async Task Login_DeveRetornar401_QuandoSenhaIncorreta()
        {
            // Arrange
            var request = new LoginRequestDto("cidadao", "senha_errada");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
