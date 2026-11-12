using AlertaComApi.Application.DTOs.Alertas;
using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Domain.Entities;

namespace AlertaComApi.Application.Mappers
{
    public static class OcorrenciaMapper
    {
        public static OcorrenciaResponseDto ToDto(Ocorrencia entity)
        {
            return new OcorrenciaResponseDto
            {
                Id = entity.Id,
                Protocolo = entity.Protocolo,
                Titulo = entity.Titulo,
                Descricao = entity.Descricao,
                TipoRisco = entity.TipoRisco.ToString(),
                Severidade = entity.Severidade.ToString(),
                Status = entity.Status.ToString(),
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Endereco = entity.Endereco,
                Bairro = entity.Bairro,
                FotoUrl = entity.FotoUrl,
                ParecerTecnico = entity.ParecerTecnico,
                UserId = entity.UserId,
                NomeCidadao = entity.User?.Nome,
                DataCriacao = entity.DataCriacao,
                DataAtualizacao = entity.DataAtualizacao
            };
        }

        public static Ocorrencia ToEntity(OcorrenciaCreateDto dto, int userId, string? idempotencyKey = null)
        {
            return new Ocorrencia(
                dto.Titulo,
                dto.Descricao,
                dto.TipoRisco,
                dto.Severidade,
                dto.Latitude,
                dto.Longitude,
                dto.Endereco,
                dto.Bairro,
                userId,
                dto.FotoUrl,
                idempotencyKey
            );
        }
    }

    public static class UserMapper
    {
        public static UserDto ToDto(User entity)
        {
            return new UserDto(
                entity.Id,
                entity.Username,
                entity.Nome,
                entity.Email,
                entity.Cpf,
                entity.Bairro,
                entity.Role,
                entity.DataCriacao
            );
        }
    }

    public static class AlertaMapper
    {
        public static AlertaClimaticoDto ToDto(AlertaClimatico entity)
        {
            return new AlertaClimaticoDto
            {
                Id = entity.Id,
                Titulo = entity.Titulo,
                Mensagem = entity.Mensagem,
                TipoRisco = entity.TipoRisco.ToString(),
                Nivel = entity.Nivel.ToString(),
                RegiaoAfetada = entity.RegiaoAfetada,
                Bairro = entity.Bairro,
                DataEmissao = entity.DataEmissao,
                DataExpiracao = entity.DataExpiracao,
                Ativo = entity.Ativo
            };
        }

        public static AlertaClimatico ToEntity(AlertaCreateDto dto)
        {
            return new AlertaClimatico(
                dto.Titulo,
                dto.Mensagem,
                dto.TipoRisco,
                dto.Nivel,
                dto.RegiaoAfetada,
                dto.Bairro,
                dto.DuracaoHoras
            );
        }
    }
}
