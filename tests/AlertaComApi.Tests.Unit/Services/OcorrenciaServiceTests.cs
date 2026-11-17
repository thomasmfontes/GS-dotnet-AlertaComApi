using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Application.Services;
using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Enums;
using AlertaComApi.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AlertaComApi.Tests.Unit.Services
{
    public class OcorrenciaServiceTests
    {
        private readonly Mock<IOcorrenciaRepository> _repoMock = new();
        private readonly Mock<IHateoasService> _hateoasMock = new();
        private readonly Mock<ILogger<OcorrenciaService>> _loggerMock = new();
        private readonly OcorrenciaService _service;

        public OcorrenciaServiceTests()
        {
            _service = new OcorrenciaService(_repoMock.Object, _hateoasMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateAsync_DeveRetornarRegistroExistente_QuandoIdempotencyKeyJaExiste()
        {
            // Arrange
            const string chaveIdempotencia = "KEY-REPETIDA-999";
            var existing = new Ocorrencia(
                "Bueiro entupido", 
                "Acúmulo de lixo", 
                TipoRisco.BueiroObstruido, 
                NivelSeveridade.Media, 
                -23.5, 
                -46.6, 
                "Rua X", 
                "Centro", 
                1, 
                idempotencyKey: chaveIdempotencia)
            {
                Id = 100
            };

            _repoMock.Setup(r => r.GetByIdempotencyKeyAsync(chaveIdempotencia))
                .ReturnsAsync(existing);

            var dto = new OcorrenciaCreateDto
            {
                Titulo = "Bueiro entupido",
                Descricao = "Acúmulo de lixo",
                TipoRisco = TipoRisco.BueiroObstruido,
                Severidade = NivelSeveridade.Media,
                Latitude = -23.5,
                Longitude = -46.6,
                Endereco = "Rua X",
                Bairro = "Centro"
            };

            // Act
            var result = await _service.CreateAsync(dto, 1, chaveIdempotencia);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(100);
            result.Message.Should().Contain("idempotência");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Ocorrencia>()), Times.Never);
        }
    }
}
