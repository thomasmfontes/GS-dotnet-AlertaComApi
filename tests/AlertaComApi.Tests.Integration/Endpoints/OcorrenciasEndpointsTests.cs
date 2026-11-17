using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Domain.Common;
using AlertaComApi.Domain.Enums;
using AlertaComApi.Middlewares;
using AlertaComApi.Tests.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace AlertaComApi.Tests.Integration.Endpoints
{
    public class OcorrenciasEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public OcorrenciasEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> ObterTokenAsync(string username = "cidadao", string password = "123456")
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(username, password));
            var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return body!.Token;
        }

        [Fact]
        public async Task GetOcorrencias_DeveRetornar200EListaPaginada()
        {
            // Act
            var response = await _client.GetAsync("/api/ocorrencias?pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PageResult<OcorrenciaResponseDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeEmpty();
            result.TotalCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task PostOcorrencia_ComIdempotencyKey_DeveEvitarDuplicacaoEmRequisicoesRepetidas()
        {
            // Arrange
            var token = await ObterTokenAsync();
            var chaveIdempotencia = "TESTE-IDEMPOTENCIA-" + Guid.NewGuid();

            var dto = new OcorrenciaCreateDto
            {
                Titulo = "Queda de poste de energia após vento",
                Descricao = "Fiação exposta oferecendo risco aos transeuntes na calçada.",
                TipoRisco = TipoRisco.QuedaArvore,
                Severidade = NivelSeveridade.Alta,
                Latitude = -23.55,
                Longitude = -46.63,
                Endereco = "Rua Teste da Silva, 500",
                Bairro = "Liberdade"
            };

            var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/ocorrencias")
            {
                Content = JsonContent.Create(dto)
            };
            request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request1.Headers.Add(IdempotencyMiddleware.IdempotencyHeader, chaveIdempotencia);

            // Act 1: Primeira requisição (deve criar)
            var response1 = await _client.SendAsync(request1);
            response1.StatusCode.Should().Be(HttpStatusCode.Created);
            var criado1 = await response1.Content.ReadFromJsonAsync<OcorrenciaResponseDto>();
            criado1.Should().NotBeNull();

            // Act 2: Segunda requisição idêntica com o mesmo header Idempotency-Key
            var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/ocorrencias")
            {
                Content = JsonContent.Create(dto)
            };
            request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request2.Headers.Add(IdempotencyMiddleware.IdempotencyHeader, chaveIdempotencia);

            var response2 = await _client.SendAsync(request2);

            // Assert
            response2.StatusCode.Should().Be(HttpStatusCode.Created);
            response2.Headers.Should().ContainKey("X-Idempotent-Replay");
            var criado2 = await response2.Content.ReadFromJsonAsync<OcorrenciaResponseDto>();
            criado2.Should().NotBeNull();
            criado2!.Id.Should().Be(criado1!.Id);
            criado2.Protocolo.Should().Be(criado1.Protocolo);
        }
    }
}
