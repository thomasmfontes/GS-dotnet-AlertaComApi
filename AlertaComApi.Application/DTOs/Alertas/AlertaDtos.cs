using AlertaComApi.Domain.Common;
using AlertaComApi.Domain.Enums;

namespace AlertaComApi.Application.DTOs.Alertas
{
    public class AlertaClimaticoDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public string TipoRisco { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public string RegiaoAfetada { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public DateTime DataExpiracao { get; set; }
        public bool Ativo { get; set; }
        public List<LinkDto> Links { get; set; } = new();
    }

    public class AlertaCreateDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public TipoRisco TipoRisco { get; set; } = TipoRisco.Alagamento;
        public NivelSeveridade Nivel { get; set; } = NivelSeveridade.Alta;
        public string RegiaoAfetada { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public int DuracaoHoras { get; set; } = 24;
    }
}
