using AlertaComApi.Domain.Enums;

namespace AlertaComApi.Domain.Entities
{
    public class AlertaClimatico
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public TipoRisco TipoRisco { get; set; } = TipoRisco.Alagamento;
        public NivelSeveridade Nivel { get; set; } = NivelSeveridade.Alta;
        public string RegiaoAfetada { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
        public DateTime DataExpiracao { get; set; } = DateTime.UtcNow.AddHours(24);
        public bool Ativo { get; set; } = true;

        public AlertaClimatico() { }

        public AlertaClimatico(string titulo, string mensagem, TipoRisco tipoRisco, NivelSeveridade nivel, string regiao, string bairro, int duracaoHoras = 24)
        {
            Titulo = titulo;
            Mensagem = mensagem;
            TipoRisco = tipoRisco;
            Nivel = nivel;
            RegiaoAfetada = regiao;
            Bairro = bairro;
            DataEmissao = DateTime.UtcNow;
            DataExpiracao = DateTime.UtcNow.AddHours(duracaoHoras);
            Ativo = true;
        }
    }
}
