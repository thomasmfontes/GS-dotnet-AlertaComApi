using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AlertaComApi.Tests.Unit.Domain
{
    public class OcorrenciaTests
    {
        [Fact]
        public void CriarOcorrencia_DeveInicializarComStatusAberta_EProtocoloValido()
        {
            // Act
            var ocorrencia = new Ocorrencia(
                titulo: "Rua alagada próximo ao córrego",
                descricao: "Chuva intensa provocou o transbordamento do córrego com lâmina de 40cm.",
                tipoRisco: TipoRisco.Alagamento,
                severidade: NivelSeveridade.Alta,
                latitude: -23.5851,
                longitude: -46.5823,
                endereco: "Rua do Córrego, 100",
                bairro: "Vila Prudente",
                userId: 1,
                fotoUrl: "https://exemplo.com/foto.jpg",
                idempotencyKey: "CHAVE-TESTE-123"
            );

            // Assert
            ocorrencia.Status.Should().Be(StatusOcorrencia.Aberta);
            ocorrencia.Protocolo.Should().StartWith("OCO-");
            ocorrencia.UserId.Should().Be(1);
            ocorrencia.IdempotencyKey.Should().Be("CHAVE-TESTE-123");
            ocorrencia.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}
