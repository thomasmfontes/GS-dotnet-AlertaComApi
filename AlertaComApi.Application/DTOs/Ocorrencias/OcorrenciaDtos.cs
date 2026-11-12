using AlertaComApi.Domain.Common;
using AlertaComApi.Domain.Enums;

namespace AlertaComApi.Application.DTOs.Ocorrencias
{
    public class OcorrenciaCreateDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public TipoRisco TipoRisco { get; set; } = TipoRisco.Alagamento;
        public NivelSeveridade Severidade { get; set; } = NivelSeveridade.Media;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Endereco { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
    }

    public class OcorrenciaUpdateStatusDto
    {
        public StatusOcorrencia NovoStatus { get; set; }
        public string? ParecerTecnico { get; set; }
    }

    public class OcorrenciaResponseDto
    {
        public int Id { get; set; }
        public string Protocolo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string TipoRisco { get; set; } = string.Empty;
        public string Severidade { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Endereco { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public string? ParecerTecnico { get; set; }
        public int UserId { get; set; }
        public string? NomeCidadao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public List<LinkDto> Links { get; set; } = new();
    }

    public class OcorrenciaFilterDto
    {
        public int? PageIndex { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
        public StatusOcorrencia? Status { get; set; }
        public TipoRisco? TipoRisco { get; set; }
        public string? Bairro { get; set; }
    }
}
