using AlertaComApi.Domain.Enums;

namespace AlertaComApi.Domain.Entities
{
    public class Ocorrencia
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public TipoRisco TipoRisco { get; set; } = TipoRisco.Alagamento;
        public NivelSeveridade Severidade { get; set; } = NivelSeveridade.Media;
        public StatusOcorrencia Status { get; set; } = StatusOcorrencia.Aberta;
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Endereco { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        
        public string Protocolo { get; set; } = string.Empty; // ex: OCO-2026-0001
        public string? IdempotencyKey { get; set; }
        public string? ParecerTecnico { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }

        public Ocorrencia() { }

        public Ocorrencia(
            string titulo,
            string descricao,
            TipoRisco tipoRisco,
            NivelSeveridade severidade,
            double latitude,
            double longitude,
            string endereco,
            string bairro,
            int userId,
            string? fotoUrl = null,
            string? idempotencyKey = null)
        {
            Titulo = titulo;
            Descricao = descricao;
            TipoRisco = tipoRisco;
            Severidade = severidade;
            Status = StatusOcorrencia.Aberta;
            Latitude = latitude;
            Longitude = longitude;
            Endereco = endereco;
            Bairro = bairro;
            UserId = userId;
            FotoUrl = fotoUrl;
            IdempotencyKey = idempotencyKey;
            DataCriacao = DateTime.UtcNow;
            Protocolo = GerarProtocolo();
        }

        private static string GerarProtocolo()
        {
            return $"OCO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        }
    }
}
