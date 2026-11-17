using AlertaComApi.Domain.Common;
using FluentAssertions;
using Xunit;

namespace AlertaComApi.Tests.Unit.Common
{
    public class OperationResultTests
    {
        [Fact]
        public void Ok_DeveRetornarSucesso_QuandoExecutadoComMensagem()
        {
            // Act
            var result = OperationResult.Ok("Operação concluída.");

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Operação concluída.");
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Fail_DeveRetornarFalha_ComListaDeErros()
        {
            // Act
            var result = OperationResult.Fail("Erro específico");

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Be("Erro específico");
        }

        [Fact]
        public void OkGenerico_DeveArmazenarDadoTipadoCorretamente()
        {
            // Act
            var result = OperationResult<int>.Ok(42);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(42);
        }
    }
}
